using Backend.Commons.Extensions;
using Backend.Commons.Helpers;
using Backend.Consts;
using Backend.Enums;
using Backend.Models;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Backend.Controllers
{
    public class DirectxController
    {
        public List<Node> Map { get; set; }

        public List<Vehicle> Vehicles { get; set; }

        private bool isDeviceInit;

        private List<CustomVertex.PositionColored[]> vertexes;
        private List<CustomVertex.PositionColored[]> edges;

        private Microsoft.DirectX.Direct3D.Font text;
        private Microsoft.DirectX.Direct3D.Line line;
        private Vector2[] lineVertexes;
        private float minX, maxX, minY, maxY;

        public DirectxController()
        {
            isDeviceInit = false;
            vertexes = new List<CustomVertex.PositionColored[]>();
            edges = new List<CustomVertex.PositionColored[]>();
        }

        public void InitMap()
        {
            minX = Map.Min(n => n.Coordinates.X);
            maxX = Map.Max(n => n.Coordinates.X);
            minY = Map.Min(n => n.Coordinates.Y);
            maxY = Map.Max(n => n.Coordinates.Y);

            foreach (var node in Map.OrderBy(n => !n.IsUnderground))
            {
                float pX = CalculateXPosition(node.Coordinates.X);
                float pY = CalculateYPosition(node.Coordinates.Y);

                vertexes.Add(
                    DirectxHelper.CreateCircle(
                        pX,
                        pY,
                        node.Type == NodeType.Stop ? VisualizationConsts.StopColor.ToArgb() : (node.IsUnderground ? VisualizationConsts.UndergroundLineColor.ToArgb() : VisualizationConsts.BasicLineColor.ToArgb()),
                        VisualizationConsts.PointRadius,
                        VisualizationConsts.PointPrecision));

                foreach (var child in node.Children)
                {
                    float pX2 = CalculateXPosition(child.Node.Coordinates.X);
                    float pY2 = CalculateYPosition(child.Node.Coordinates.Y);
                    edges.Add(DirectxHelper.CreateLine(pX, pY, pX2, pY2, GetLineColor(node, child.Node).ToArgb(), VisualizationConsts.PointRadius));
                }
            }
        }

        public void Render(Device device, Vector3 cameraPosition, string time)
        {
            if (!isDeviceInit)
                InitDevice(device);

            //DRAW EDGES
            foreach (var edge in edges)
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, edge);

            //DRAW POINTS
            foreach (var vertex in vertexes)
                device.DrawUserPrimitives(PrimitiveType.TriangleFan, VisualizationConsts.PointPrecision, vertex);

            DrawVehicles(device, cameraPosition);

            //DRAW TIME
            text.DrawText(null, time, new Point(12, 11), Color.Black);
            line.Draw(lineVertexes, Color.Black);
        }

        public float CalculateXPosition(float originalX) => 
            (100 - (originalX - minX) * 100 / (maxX - minX)) - 50;

        public float CalculateYPosition(float originalY) => 
            (originalY - minY) * 100 / (maxX - minX) - (50 * (minY - maxY)) / (minX - maxX);

        private void InitDevice(Device device)
        {
            System.Drawing.Font systemfont = new System.Drawing.Font("Arial", 12f, FontStyle.Regular);
            text = new Microsoft.DirectX.Direct3D.Font(device, systemfont);
            line = new Microsoft.DirectX.Direct3D.Line(device);
            lineVertexes = new Vector2[] { new Vector2(8, 8), new Vector2(77, 8), new Vector2(77, 31), new Vector2(8, 31), new Vector2(8, 8) };
            isDeviceInit = true;
        }

        private void DrawVehicles(Device device, Vector3 cameraPosition)
        {
            foreach (var vehicle in Vehicles)
            {
                Color tramColor = Color.Green;
                float pX = CalculateXPosition(vehicle.Position.Coordinates.X);
                float pY = CalculateYPosition(vehicle.Position.Coordinates.Y);
                float thickness = GetPointRadius(cameraPosition.Z);

                device.DrawUserPrimitives(PrimitiveType.TriangleFan, VisualizationConsts.PointPrecision, DirectxHelper.CreateCircle(pX, pY, tramColor.ToArgb(), thickness, VisualizationConsts.PointPrecision));

                float length = VehicleConsts.LenghtInM;
                int actualNodeIndex = vehicle.VisitedNodes.Count - 2;
                Vector2 prevCoordinates = vehicle.Position.Coordinates;
                float pX2, pY2;
                while (length > 0)
                {
                    if (actualNodeIndex >= 0)
                    {
                        Node actualNode = vehicle.VisitedNodes[actualNodeIndex--];
                        float distance = prevCoordinates.RealDistanceTo(actualNode.Coordinates);
                        pX = CalculateXPosition(prevCoordinates.X);
                        pY = CalculateYPosition(prevCoordinates.Y);
                        if (distance >= length)
                        {
                            float displacement = (distance - length) * 100 / distance;
                            var pos = GeometryHelper.GetLocactionBetween(displacement, actualNode.Coordinates, prevCoordinates);
                            pX2 = CalculateXPosition(pos.X);
                            pY2 = CalculateYPosition(pos.Y);
                        }
                        else
                        {
                            pX2 = CalculateXPosition(actualNode.Coordinates.X);
                            pY2 = CalculateYPosition(actualNode.Coordinates.Y);
                            prevCoordinates = actualNode.Coordinates;
                        }

                        var tramTail = DirectxHelper.CreateLine(pX, pY, pX2, pY2, tramColor.ToArgb(), thickness);
                        device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, tramTail);

                        length -= distance;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private float GetPointRadius(float cameraHeight) => 
            (cameraHeight * (19f / 99) + (80f / 99)) * VisualizationConsts.PointRadius;

        private static Color GetLineColor(Node node1, Node node2) => 
            node1 != null && node1.IsUnderground && node2 != null && node2.IsUnderground ? VisualizationConsts.UndergroundLineColor : VisualizationConsts.BasicLineColor;
    }
}
