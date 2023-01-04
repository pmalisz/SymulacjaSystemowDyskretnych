using Backend.Commons.Helpers;
using Backend.Consts;
using Backend.Models;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Backend.Controllers
{
    public class MainController
    {
        private FileController fileController;
        private DirectxController directxController;
        private VehicleController vehicleController;

        public DateTime ActualRealTime { get; set; }

        public MainController(DirectxController directxController, VehicleController vehicleContrller, FileController fileController)
        {
            this.directxController = directxController;
            this.vehicleController = vehicleContrller;
            this.fileController = fileController;
            
            fileController.LoadData();
        }

        public void StartSimulation(DateTime startTime)
        {
            vehicleController.ActualRealTime = startTime;
            GetAndPrepareModels();
        }

        public void Render(Device device, Vector3 cameraPosition)
        {
            directxController.Render(device, cameraPosition, TimeHelper.GetLongTimeAsString(vehicleController.ActualRealTime));
        }

        public void Update()
        {
            float deltaTime = ApplicationConsts.TimeIntervalInS * ApplicationConsts.SimulationSpeed;

            vehicleController.Update(deltaTime);

            vehicleController.Vehicles.RemoveAll(vehicleController.FinishCoursePredicate);

            StartNewCourses();
        }

        private void GetAndPrepareModels()
        {
            directxController.Map = fileController.Nodes;
            directxController.Vehicles = vehicleController.Vehicles = new List<Vehicle>();
            directxController.InitMap();
        }

        private void StartNewCourses()
        {
            List<Node> startPoints = new List<Node>();
            foreach (var line in fileController.Lines)
            {
                for (int i = line.Departures.Count - 1; i >= 0; i--)
                {
                    if (TimeHelper.GetShortTimeAsString(line.Departures[i]) == TimeHelper.GetShortTimeAsString(vehicleController.ActualRealTime))
                    {
                        if (line.Departures[i] != line.LastDepartureTime &&
                            !startPoints.Any(sp => sp.Equals(line.Nodes.First())) &&
                            vehicleController.IsFreeSpace(line.Nodes.First(), VehicleConsts.SafeSpaceInM))
                        {
                            startPoints.Add(line.Nodes.First());
                            line.LastDepartureTime = line.Departures[i];
                            Vehicle newVehicle = new Vehicle(line);
                            
                            line.Nodes.First().VehiclesOn.Add(newVehicle);
                            vehicleController.Vehicles.Add(newVehicle);
                        }

                        break;
                    }
                }
            }
        }
    }
}
