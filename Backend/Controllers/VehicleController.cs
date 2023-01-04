using Backend.Commons.Helpers;
using Backend.Consts;
using Backend.Models;
using Backend.Enums;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Backend.Controllers
{
    public class VehicleController
    {
        private FileController fileController;

        public List<Vehicle> Vehicles { get; set; }

        public DateTime ActualRealTime { get; set; }

        public VehicleController(FileController fileController)
        {
            this.fileController = fileController;
        }

        public void Update(float deltaTime)
        {
            ActualRealTime += new TimeSpan(0, 0, 0, 0, (int)(deltaTime * 1000));
            foreach (var vehicle in Vehicles)
            {
                float prevSpeed = vehicle.Speed;
                CalculateSpeed(vehicle, deltaTime);
                if (!vehicle.IsOnStop)
                    CalculatePosition(vehicle, prevSpeed, deltaTime);

                if (FinishCoursePredicate(vehicle))
                {
                    if(vehicle.Position.Node1.Type == NodeType.Stop)
                    {
                        vehicle.VisitedStops.Add(vehicle.Position.Node1);
                        vehicle.DepartureTimes.Add(ActualRealTime);
                    }

                    fileController.ExportData(vehicle);
                    vehicle.Position.Node1.VehiclesOn.Remove(vehicle);
                    return;
                }
            }
        }

        public bool IsFreeSpace(Node node, float length) => 
            !Vehicles.Any(v => GeometryHelper.GetRealDistance(node.Coordinates, v.Position.Coordinates) <= length);

        public bool FinishCoursePredicate(Vehicle vehicle) => 
            vehicle.Position.Node1.Equals(vehicle.Line.Nodes.Last()) ||
            (Math.Abs(vehicle.Speed) < CalculationConsts.Epsilon && vehicle.Position.Node2.Equals(vehicle.Line.Nodes.Last())) ||
            vehicle.Line.GetNextNode(vehicle.Position.Node2) == null;

        private void CalculateSpeed(Vehicle vehicle, float deltaTime)
        {
            Intersection intersection;
            if (vehicle.Speed < CalculationConsts.Epsilon && !vehicle.IsOnStop && vehicle.IsStopReached())
                HandleVehicleArrivingOnStop(vehicle);
            else if (vehicle.Speed < CalculationConsts.Epsilon && !vehicle.IsOnStop && vehicle.IsIntersectionReached(out intersection))
                HandleVehicleOnIntersection(vehicle, intersection, deltaTime);
            else if (vehicle.IsOnStop)
                HandleVehicleOnStop(vehicle, deltaTime);
            else if (vehicle.IsAnyVehicleClose(deltaTime) || !vehicle.IsStraightRoad(deltaTime))
                vehicle.Speed = vehicle.GetNewSpeed(deltaTime, false);
            else if (vehicle.CurrentIntersection != null)
                vehicle.Speed = vehicle.GetNewSpeed(deltaTime, maxSpeed: VehicleConsts.MaxCrossSpeedInKmPerH);
            else
                vehicle.Speed = vehicle.GetNewSpeed(deltaTime);
        }

        private void HandleVehicleArrivingOnStop(Vehicle vehicle)
        {
            vehicle.IsOnStop = true;
            vehicle.Speed = 0;
        }

        private void HandleVehicleOnIntersection(Vehicle vehicle, Intersection intersection, float deltaTime)
        {
            if (vehicle.CurrentIntersection != null && !intersection.Equals(vehicle.CurrentIntersection))
            {
                DequeueIntersection(vehicle.CurrentIntersection);
                vehicle.LastIntersection = vehicle.CurrentIntersection;
                vehicle.CurrentIntersection = null;
            }

            if ((intersection.CurrentVehicle == null && !intersection.WaitingVehicles.Any()) || intersection.CurrentVehicle.Equals(vehicle))
            {
                intersection.CurrentVehicle = vehicle;
                vehicle.CurrentIntersection = intersection;
                vehicle.Speed = vehicle.GetNewSpeed(deltaTime, maxSpeed: VehicleConsts.MaxCrossSpeedInKmPerH);
            }
            else if (!intersection.WaitingVehicles.Any(v => v.Equals(vehicle)))
            {
                intersection.WaitingVehicles.Enqueue(vehicle);
            }
        }

        private void HandleVehicleOnStop(Vehicle vehicle, float deltaTime)
        {
            if (vehicle.Speed < CalculationConsts.Epsilon)
            {
                vehicle.IsOnStop = false;
                var lastVisitedStop = vehicle.Position.Node1 != null && vehicle.Position.Node1.Type == NodeType.Stop && vehicle.LastVisitedStop != vehicle.Position.Node1 ? vehicle.Position.Node1 : vehicle.Position.Node2;
                vehicle.VisitedStops.Add(lastVisitedStop);

                vehicle.Speed = vehicle.GetNewSpeed(deltaTime);
                vehicle.DepartureTimes.Add(ActualRealTime);
            }
            else
                vehicle.Speed = vehicle.GetNewSpeed(deltaTime, false);
        }

        private void CalculatePosition(Vehicle vehicle, float prevSpeed, float deltaTime)
        {
            float translation = PhysicsHelper.GetTranslation(prevSpeed, vehicle.Speed, deltaTime);
            if (vehicle.Position.Node2 != null)
            {
                float distanceToNextPoint = vehicle.RealDistanceTo(vehicle.Position.Node2.Coordinates);
                if (distanceToNextPoint > translation)
                {
                    vehicle.Position.Displacement += translation * 100 / vehicle.Position.Node1.GetDistanceToChild(vehicle.Position.Node2);
                }
                else
                {
                    Node.Next newNode = vehicle.Line.GetNextNode(vehicle.Position.Node2);
                    vehicle.VisitedNodes.Add(newNode.Node);
                    vehicle.Position.Node1.VehiclesOn.Remove(vehicle);
                    vehicle.Position.Node1 = vehicle.Position.Node2;
                    vehicle.Position.Node1.VehiclesOn.Add(vehicle);
                    vehicle.Position.Node2 = newNode.Node;
                    vehicle.Position.Displacement = 0;
                    vehicle.Position.Displacement += (translation - distanceToNextPoint) * 100 / vehicle.Position.Node1.GetDistanceToChild(vehicle.Position.Node2);
                }
            }

            if (translation > 0)
            {
                vehicle.Position.Coordinates = vehicle.Position.Node2 == null || vehicle.Position.Displacement < CalculationConsts.Epsilon ?
                                               vehicle.Position.Node1.Coordinates :
                                               GeometryHelper.GetLocactionBetween(vehicle.Position.Displacement, vehicle.Position.Node1.Coordinates, vehicle.Position.Node2.Coordinates);
            }

            if (vehicle.CurrentIntersection != null && (!vehicle.IsStillOnIntersection() || FinishCoursePredicate(vehicle)))
            {
                DequeueIntersection(vehicle.CurrentIntersection);
                vehicle.LastIntersection = vehicle.CurrentIntersection;
                vehicle.CurrentIntersection = null;
            }
        }

        private void DequeueIntersection(Intersection intersection)
        {
            intersection.CurrentVehicle = intersection.WaitingVehicles.Any() ? intersection.WaitingVehicles.Dequeue() : null;
        }
    }
}
