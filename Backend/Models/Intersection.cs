using System.Collections.Generic;

namespace Backend.Models
{
    public class Intersection : BaseModel
    {
        public Intersection(string id)
        {
            Id = id;
            Nodes = new List<Node>();
            WaitingVehicles = new Queue<Vehicle>();
        }

        public List<Node> Nodes { get; set; }

        public Vehicle CurrentVehicle { get; set; }

        public Queue<Vehicle> WaitingVehicles { get; set; }
    }
}
