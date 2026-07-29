using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.DeviceMonitoring
{
    public class IoTHubSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int CheckIntervalSeconds { get; set; } = 30;
        public int TimeoutSeconds { get; set; } = 10;
    }
}
