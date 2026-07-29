using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.DeviceParameter
{
    public class SetIceMakerMachineParameter
    {
        public string? Language { get; set; } = "";
        public double? IceQuantity { get; set; }
        public double? WaterQuantity { get; set; }
        public double? IceWaterQuantity { get; set; }
    }
}
