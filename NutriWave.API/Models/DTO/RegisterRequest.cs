using System.ComponentModel.DataAnnotations;

namespace NutriWave.API.Models.DTO;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; set; }

    [Required, MaxLength(100)]
    public required string LastName { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required DateTime BirthDate { get; set; }
}