using System.ComponentModel.DataAnnotations;
using CouchDB.Driver.Types;

namespace Domain.CouchDbModels
{
    public class DeviceStatusDocument : CouchDocument
    {
        [Key]
        [StringLength(50)]
        public string DeviceId { get; set; } = null!;
        public Dictionary<string, object> Status { get; set; }
        public Dictionary<string, string> Labels { get; set; } = [];

        public DateTime LastUpdated { get; set; }
    }
}
