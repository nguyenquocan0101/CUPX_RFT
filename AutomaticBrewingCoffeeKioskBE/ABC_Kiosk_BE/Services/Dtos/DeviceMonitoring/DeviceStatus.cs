using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.DeviceMonitoring
{
    public class DeviceStatus
    {
        [Key]
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime LastChecked { get; set; }
        public string ConnectionState { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
    }

}
