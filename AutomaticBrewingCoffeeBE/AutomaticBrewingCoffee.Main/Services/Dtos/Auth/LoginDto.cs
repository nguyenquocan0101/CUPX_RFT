using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Auth;

public class LoginDto
{
    [Required] public string Email { get; set; }
    [Required] public string Password { get; set; }
}