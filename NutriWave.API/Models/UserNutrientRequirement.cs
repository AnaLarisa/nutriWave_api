using System.ComponentModel.DataAnnotations;

namespace NutriWave.API.Models;

public class UserNutrientRequirement
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int NutrientId { get; set; }

    [Required]
    public int Quantity { get; set; }

    public UserInformation User { get; set; } = null!;
    public Nutrient Nutrient { get; set; } = null!;
}
