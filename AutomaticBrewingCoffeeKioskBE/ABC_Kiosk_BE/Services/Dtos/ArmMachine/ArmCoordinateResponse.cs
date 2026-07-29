using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.ArmMachine
{
    public class ArmCoordinateResponse
    {
        public string InformationType { get; set; }
        public Coordinate Coordinate { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class Coordinate
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float RX { get; set; }
        public float RY { get; set; }
        public float RZ { get; set; }
        public float J1 { get; set; }
        public float J2 { get; set; }
        public float J3 { get; set; }
        public float J4 { get; set; }
        public float J5 { get; set; }
        public float J6 { get; set; }
    }
}
