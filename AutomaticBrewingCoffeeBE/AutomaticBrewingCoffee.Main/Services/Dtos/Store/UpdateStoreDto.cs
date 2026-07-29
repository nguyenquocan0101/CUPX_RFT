using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.Organization;

namespace Services.Dtos.Store;

public class UpdateStoreDto
{
    [StringLength(50)] [Required] public string OrganizationId { get; set; } = null!;

    public virtual OrganizationDto? Organization { get; set; }

    [StringLength(100)] public string? ContactPhone { get; set; }

    [StringLength(100)] public string? Name { get; set; }

    [StringLength(300)] public string? Description { get; set; }

    [StringLength(450)] public string? LocationAddress { get; set; } = null!;

    [StringLength(50)] public string? LocationTypeId { get; set; } = null!;

    [StringLength(10)] [Required] public string Status { get; set; } = null!;
}