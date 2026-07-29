using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Cludflare.Models
{
    public class CloudflareTunnel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TunnelSecret { get; set; }
        public string TunnelId { get; set; }
        public string AccountTag { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string ConfigSrc { get; set; }
        public string Status { get; set; }
    }
}
