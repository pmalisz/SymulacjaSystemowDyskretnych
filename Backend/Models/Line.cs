using System.Collections.Generic;
using System;
using Backend.Enums;
using System.Linq;

namespace Backend.Models
{
    public class Line : BaseModel
    {
        public Line(string id)
        {
            Id = id;
            Departures = new List<DateTime>();
            Nodes = new List<Node>();
        }

        public List<Node> Nodes { get; set; }

        public List<DateTime> Departures { get; set; }

        public DateTime LastDepartureTime { get; set; }

        public Node.Next GetNextNode(Node node)
        {
            if (node.Children != null && node.Children.Any())
            {
                if (node.Children.Count == 1)
                    return node.Children.First();

                return node.Children.Single(ch => Nodes.Any(n => n.Equals(ch.Node)));
            }

            return null;
        }

        public float GetNextStopDistance(Node node)
        {
            Node.Next next = GetNextNode(node);
            float distance = next.Distance;
            while (!(next.Node.Type == NodeType.Stop && Nodes.Any(n => n.Equals(next.Node))))
            {
                next = GetNextNode(next.Node);
                distance += next.Distance;
            }

            return distance;
        }
    }
}
