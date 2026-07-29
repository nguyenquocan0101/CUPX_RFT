using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Webhook : BaseModel
{
    [Key] [StringLength(50)] [Required] public string WebhookId { get; set; } = null!;

    [StringLength(50)] public string KioskId { get; set; } = null!;

    [ForeignKey(nameof(KioskId))] public Kiosk? Kiosk { get; set; } = null!;

    [StringLength(450)] public string WebhookUrl { get; set; } = null!;

    [StringLength(100)] public string WebhookType { get; set; } = null!;
}