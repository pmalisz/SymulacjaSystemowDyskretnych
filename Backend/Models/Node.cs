using System.Collections.Generic;
using Microsoft.DirectX;
using Backend.Enums;
using System.Linq;

namespace Backend.Models
{
    public class Node : BaseModel
    {
        public Node()
        {
            Children = new List<Next>();
            VehiclesOn = new List<Vehicle>();
        }

        public Vector2 Coordinates { get; set; }

        public NodeType Type { get; set; }

        public string StopName { get; set; }

        public Intersection Intersection { get; set; }

        public bool IsUnderground { get; set; }

        public List<Next> Children { get; set; }

        public List<Vehicle> VehiclesOn { get; set; }

        public class Next
        {
            public Node Node { get; set; }

            public float Distance { get; set; }
        }

        public float GetDistanceToChild(Node child) => Children.Single(n => n.Node.Equals(child)).Distance;
    }
}
