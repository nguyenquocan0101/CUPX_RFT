using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.DeviceParameter
{
    public class SetDeviceParameterDto
    {
        [Required]
        public List<DeviceParameter> DeviceParamsList { get; set; }
    }

    public class DeviceParameter
    {
        public string DeviceId { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;
    }
}
