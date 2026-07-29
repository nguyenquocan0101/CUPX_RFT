using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.Sync
{
    public class DeviceSyncDto
    {
        public string DeviceId { get; set; } = null!;
        public string? DeviceModelId { get; set; }
        public string SerialNumber { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Dictionary<string, object> DictionaryStatus { get; set; } = new Dictionary<string, object>();
    }
}
