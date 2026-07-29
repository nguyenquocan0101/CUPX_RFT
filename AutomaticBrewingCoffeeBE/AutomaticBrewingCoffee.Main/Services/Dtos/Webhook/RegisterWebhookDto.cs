using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Webhook;

public class RegisterWebhookDto
{
    [StringLength(50)] public string? KioskId { get; set; }

    [StringLength(450)] public string? WebhookUrl { get; set; }

    [MatchEnum(typeof(EWebhookType))] public string? WebhookType { get; set; }
}