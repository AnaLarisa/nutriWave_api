using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NutriWave.API.Models;

public class UserNutrientIntake
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required int UserId { get; set; }

    [Required]
    public required int NutrientId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public UserInformation User { get; set; } = null!;
    public Nutrient Nutrient { get; set; } = null!;
}
