using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.DeviceParameter
{
    public class ParameterValue
    {
        public object Value { get; set; }
        public bool IsSetting { get; set; }
    }

    public class DeviceParameterDto
    {
        public DeviceType DeviceType { get; set; }
        public Dictionary<string, ParameterValue> Parameters { get; set; } = new Dictionary<string, ParameterValue>();
    }
}
