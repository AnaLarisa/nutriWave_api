using System.ComponentModel.DataAnnotations;

namespace NutriWave.API.Models;

public class UserInformation
{
    [Key]
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public required string FirstName { get; set; }

    [Required, MaxLength(100)]
    public required string LastName { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }

    [MaxLength(255)]
    public required string PasswordHash { get; set; }

    public string[]? MedicalConditions { get; set; }

    [MaxLength(500)]
    public string? MedicalReportUrl { get; set; }

    public List<UserNutrientIntake> NutrientIntakes { get; set; } = [];

    public List<UserNutrientRequirement> NutrientRequirements { get; set; } = [];
}

