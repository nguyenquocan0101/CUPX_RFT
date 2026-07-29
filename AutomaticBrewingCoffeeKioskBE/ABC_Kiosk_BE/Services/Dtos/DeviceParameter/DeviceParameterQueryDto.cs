using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.DeviceParameter
{
    public class DeviceParameterQueryDto
    {
        [Required(ErrorMessage = "Device id is required.")]
        public string DeviceId { get; set; }
    }
}
