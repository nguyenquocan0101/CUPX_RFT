
namespace ArmController2
{
    public class ArmCoordinate
    {
        //public string KioskId { get; set; }
        //public string Flag { get; set; }
        public string InformationType { get; set; }
        public Coordinate Coordinate { get; set; }
        public System.DateTime TimeStamp { get; set; }
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
