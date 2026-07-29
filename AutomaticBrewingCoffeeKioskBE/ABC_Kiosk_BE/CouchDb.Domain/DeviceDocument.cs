using System.ComponentModel.DataAnnotations;
using CouchDB.Driver.Types;
using CouchDb.Domain.Enums;

namespace Domain.CouchDbModels
{
    public class DeviceDocument : CouchDocument
    {
        [Key]
        [StringLength(50)]
        public string DeviceId { get; set; } = null!;
        [StringLength(50)] public string? DeviceModelId { get; set; }

        [Required]
        [StringLength(50)]
        public string SerialNumber { get; set; } = null!;
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(300)]
        public string Description { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public EWorkingStatus WorkingStatus { get; set; }

        public Dictionary<string, object> Status { get; set; }
        public Dictionary<string, string> Labels { get; set; }
    }
}
