using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IceMakerDevice.Dtos
{
    public class SetParamsPayload
    {
        public string Language { get; set;}
        public double IceQty { get; set;} 
        public double WaterQty { get; set;}
        public double IceWaterQty { get; set;}
    }
}
