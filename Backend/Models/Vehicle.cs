using Backend.Commons.Helpers;
using Backend.Consts;
using Backend.Enums;
using Microsoft.DirectX;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Backend.Models
{
    public class Vehicle : BaseModel
    {
        public Vehicle()
        {
            DepartureTimes = new List<DateTime>();
            VisitedStops = new List<Node>();
            VisitedNodes = new List<Node>();
        }

        public Vehicle(Line line) : this()
        {
            Id = TimeHelper.GetShortTimeAsString(line.LastDepartureTime) + " - " + line.Id;
            Line = line;
            StartTime = line.LastDepartureTime;
            Speed = 0;
            IsOnStop = line.Nodes.First().Type == NodeType.Stop;
            VisitedNodes.Add(line.Nodes.First());
            VisitedNodes.Add(line.Nodes.First().Children.Last().Node);
            Position = new Location()
            {
                Node1 = line.Nodes.First(),
                Node2 = line.Nodes.First().Children.Last().Node,
                Displacement = 0,
                Coordinates = line.Nodes.First().Coordinates
            };
        }

        public Line Line { get; set; }

        public DateTime StartTime { get; set; }

        public List<DateTime> DepartureTimes { get; set; }

        public bool IsOnStop { get; set; }

        public Location Position { get; set; }

        public float Speed { get; set; }

        public List<Node> VisitedStops { get; set; }

        public Node LastVisitedStop => VisitedStops.LastOrDefault();

        public Intersection CurrentIntersection { get; set; }

        public Intersection LastIntersection { get; set; }

        public List<Node> VisitedNodes { get; set; }

        public class Location
        {
            public Node Node1 { get; set; }

            public Node Node2 { get; set; }

            public float Displacement { get; set; }

            public Vector2 Coordinates { get; set; }
        }

        public float GetNewSpeed(float deltaTime, bool increase = true, float maxSpeed = VehicleConsts.MaxSpeedInKmPerH)
        {
            float newSpeed = PhysicsHelper.GetNewSpeed(Speed, deltaTime, increase);
            return increase ? Math.Min(newSpeed, maxSpeed) : Math.Max(newSpeed, 0);
        }

        public float RealDistanceTo(Vector2 coordinates) =>
            GeometryHelper.GetRealDistance(Position.Coordinates, coordinates);

        public bool IsStopReached()
        {
            var node1 = Position.Node1;
            var node2 = Position.Node2;
            return IsStopReachedForNode(node1) || IsStopReachedForNode(node2);
        }

        private bool IsStopReachedForNode(Node node) => 
            node != null &&
            IsNodeCorrectStop(node) &&
            RealDistanceTo(node.Coordinates) <= CalculationConsts.DistanceEpsilon;

        public bool IsIntersectionReached(out Intersection intersection)
        {
            var node1 = Position.Node1;
            var node2 = Position.Node2;
            if (IsIntersectionReachedForNode(node1))
            {
                intersection = node1.Intersection;
                return true;
            }

            if (IsIntersectionReachedForNode(node2))
            {
                intersection = node2.Intersection;
                return true;
            }

            intersection = null;
            return false;
        }

        private bool IsIntersectionReachedForNode(Node node) => 
            node != null && 
            node.Type == NodeType.Intersection && 
            !node.Intersection.Equals(LastIntersection);

        public bool IsStillOnIntersection() => 
            CurrentIntersection.Nodes.Any(n => (RealDistanceTo(n.Coordinates) - VehicleConsts.LenghtInM) < 0) ||
            CurrentIntersection.Equals(Position.Node2?.Intersection);

        public bool IsStraightRoad(float deltaTime)
        {
            var node1 = Position.Node1;
            if (node1 != null && IsNodeCorrectStop(node1))
                return false;

            var node2 = Position.Node2;
            if (node2 == null)
                return true;

            float speed = GetNewSpeed(deltaTime);
            float distance = RealDistanceTo(node2.Coordinates) - 1;
            float brakingDistance = PhysicsHelper.GetBrakingDistance(speed);

            if (IsNotStraightRoad(node2, distance, brakingDistance))
                return false;

            while (distance <= brakingDistance)
            {
                var newNode = Line.GetNextNode(node2);
                if (newNode == null)
                    return true;

                distance += newNode.Distance;
                node2 = newNode.Node;
                if (IsNotStraightRoad(node2, distance, brakingDistance))
                    return false;
            }

            return true;
        }

        private bool IsNotStraightRoad(Node node, float distance, float brakingDistance) => 
            distance <= brakingDistance && 
            node.Type != NodeType.Normal && 
            (IsNodeCorrectIntersection(node) || IsNodeCorrectStop(node));

        public bool IsAnyVehicleClose(float deltaTime)
        {
            float speed = GetNewSpeed(deltaTime);
            float brakingDistance = PhysicsHelper.GetBrakingDistance(speed);

            return IsAnyVehicleClose(speed, brakingDistance, Position.Node1, 0);
        }

        private bool IsAnyVehicleClose(float speed, float brakingDistance, Node node, float distance)
        {
            if (distance > brakingDistance + VehicleConsts.SafeSpaceInM)
                return false;

            bool isFirstComparation = !(distance > 0);

            foreach (Vehicle neighbor in node.VehiclesOn)
            {
                if (!neighbor.Equals(this) &&
                        RealDistanceTo(neighbor.Position.Coordinates) <= (brakingDistance + VehicleConsts.SafeSpaceInM) &&
                        (!isFirstComparation ||
                        neighbor.RealDistanceTo(neighbor.Position.Node2.Coordinates) <= RealDistanceTo(neighbor.Position.Node2.Coordinates) &&
                        RealDistanceTo(Position.Node1.Coordinates) <= neighbor.RealDistanceTo(Position.Node1.Coordinates)))
                    return true;
            }

            return node.Children.Any(nn => IsAnyVehicleClose(speed, brakingDistance, nn.Node, isFirstComparation ? RealDistanceTo(nn.Node.Coordinates) : distance + nn.Distance));
        }

        private bool IsNodeCorrectStop(Node node) => 
            node.Type == NodeType.Stop &&
            !VisitedStops.Any(n => n.Equals(node)) &&
            Line.Nodes.Any(n => n.Equals(node));

        private bool IsNodeCorrectIntersection(Node node) => 
            node.Type == NodeType.Intersection &&
            (CurrentIntersection == null || !CurrentIntersection.Equals(node.Intersection));
    }
}
