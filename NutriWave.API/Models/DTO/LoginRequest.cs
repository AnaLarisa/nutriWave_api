using System.ComponentModel.DataAnnotations;

namespace NutriWave.API.Models.DTO;

public class LoginRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
