using Backend.Commons.Helpers;
using Backend.Models;
using Backend.Enums;
using Microsoft.DirectX;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System;
using System.Runtime.InteropServices;
using Backend.Consts;

namespace Backend.Controllers
{
    public class FileController
    {
        private string mapFilePath, linesFilePath;
        public List<Intersection> Intersections { get; set; }
        public List<Line> Lines { get; set; }
        public List<Node> Nodes { get; set; }

        public FileController()
        {
            Intersections = new List<Intersection>();
            Lines = new List<Line>();
            Nodes = new List<Node>();
        }

        public void LoadData()
        {
            var fileNames = GetFileNames();

            mapFilePath = fileNames.Item1;
            linesFilePath = fileNames.Item2;

            LoadMapAndIntersections();
            LoadLines();
        }

        public void ExportData(Vehicle vehicle)
        {
            var data = new StringBuilder();
            data.AppendLine($"{vehicle.Id};");
            for(int i=0; i<vehicle.VisitedStops.Count; i++)
                data.AppendLine($"{vehicle.VisitedStops[i].StopName};{vehicle.DepartureTimes[i].Hour}:{vehicle.DepartureTimes[i].Minute}:{vehicle.DepartureTimes[i].Second}");
            data.AppendLine(";");

            File.AppendAllText(GetBasePath() + ApplicationConsts.ResultFileName, data.ToString(), Encoding.UTF8);
        }

        // hack af but idk anymore
        private Tuple<string, string> GetFileNames()
        {
            string basePath = GetBasePath();
            var pointsPath = basePath + ApplicationConsts.PointsFileName;
            var linesPath = basePath + ApplicationConsts.LinesFileName;

            return Tuple.Create(pointsPath, linesPath);
        }

        private string GetBasePath()
        {
            var list = Environment.CurrentDirectory.Split(@"\".ToCharArray()).TakeWhile(x => x != "Visualization").ToList();
            list.Add("Resources");
            return string.Join(@"/", list.ToArray());
        }

        private void LoadMapAndIntersections()
        {
            List<string> childrenStr = new List<string>();

            using (var file = new StreamReader(mapFilePath))
            {
                file.ReadLine();

                foreach (string line in GetFileLines(file))
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] par = line.Split(';');

                        Node node = new Node()
                        {
                            Coordinates = new Vector2(float.Parse(par[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(par[1], CultureInfo.InvariantCulture.NumberFormat)),
                            Id = par[2],
                            IsUnderground = float.Parse(par[6], CultureInfo.InvariantCulture.NumberFormat) == 1,
                            VehiclesOn = new List<Vehicle>(),
                            Type = float.Parse(par[4], CultureInfo.InvariantCulture.NumberFormat) == 1 ? NodeType.Stop : !string.IsNullOrEmpty(par[3]) ? NodeType.Intersection : NodeType.Normal,
                            StopName = par[5]
                        };

                        StringBuilder childStr = new StringBuilder(";");
                        for (int i = 7; i < par.Length; i++)
                        {
                            childStr.Append(par[i]);
                            childStr.Append(";");
                        }

                        childStr.Append(";");
                        childStr.Replace("\"", "");
                        childrenStr.Add(childStr.ToString());
                        Nodes.Add(node);

                        string intersectionId = par[3];
                        if (!string.IsNullOrEmpty(intersectionId))
                        {
                            if (!Intersections.Any(i => i.Id.Equals(intersectionId))) 
                                Intersections.Add(new Intersection(intersectionId));

                            var intersection = Intersections.Single(i => i.Id.Equals(intersectionId));
                            intersection.Nodes.Add(node);
                            node.Intersection = intersection;
                        }
                    }
                }

                for (int i = 0; i < Nodes.Count; i++)
                {
                    var children = Nodes.Where(n => childrenStr[i].Contains(";" + n.Id + ";"));
                    if (children != null && children.Any())
                    {
                        foreach (var child in children)
                        {
                            Nodes[i].Children.Add(new Node.Next()
                            {
                                Node = child,
                                Distance = GeometryHelper.GetRealDistance(Nodes[i].Coordinates, child.Coordinates)
                            });
                        }
                    }
                }
            }

            Nodes = Nodes.OrderBy(n => int.Parse(n.Id)).ToList();
        }

        private void LoadLines()
        {
            using (var file = new StreamReader(linesFilePath))
            {
                Line tramLine = null;
                bool isNewTramLine = true;
                bool isDepartureLine = false;

                foreach (string line in GetFileLines(file))
                {
                    string[] par = line.Split(';');
                    if (isNewTramLine)
                    {
                        tramLine = new Line(par[0] + " (" + par[1].Replace("  ", " ").Trim() + ")");
                        isNewTramLine = false;
                    }
                    else if (string.IsNullOrEmpty(par[0]) && !isDepartureLine)
                    {
                        isDepartureLine = true;
                    }
                    else if (string.IsNullOrEmpty(par[0]) && isDepartureLine)
                    {
                        isNewTramLine = true;
                        isDepartureLine = false;

                        Lines.Add(tramLine);
                    }
                    else if (isDepartureLine)
                    {
                        for (int i = 0; i < par.Length; i++)
                        {
                            if (string.IsNullOrEmpty(par[i]))
                                break;

                            tramLine.Departures.Add(TimeHelper.GetTimeFromString(par[i]));
                        }
                    }
                    else
                    {
                        tramLine.Nodes.Add(Nodes.Single(n => n.Id == par[0]));
                    }
                }
            }
        }

        private IEnumerable<string> GetFileLines(StreamReader file)
        {
            string textLine;
            while ((textLine = file.ReadLine()) != null)
                yield return textLine;
        }
    }
}
