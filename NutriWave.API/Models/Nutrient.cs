using System.ComponentModel.DataAnnotations;

namespace NutriWave.API.Models;

public class Nutrient
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }

    [Required, MaxLength(20)]
    public required string Unit { get; set; }

    public List<UserNutrientIntake> Intakes { get; set; } = [];
    public List<UserNutrientRequirement> Requirements { get; set; } = [];
}
