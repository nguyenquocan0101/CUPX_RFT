using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Auth;

public class CreateAccountDto
{
    [Required] [StringLength(100)] public string? FullName { get; set; } = null!;

    [Required]
    [StringLength(150)]
    [MatchEmail]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(150)]
    [MatchPassword]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [MatchEnum(typeof(ERoleName))]
    public string RoleName { get; set; } = null!;

    [StringLength(50)] public string? ReferenceId { get; set; } = null!;
}