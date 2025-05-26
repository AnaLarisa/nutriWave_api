using System.ComponentModel.DataAnnotations;

namespace NutriWave.API.Models;

public class SportLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required int UserId { get; set; }

    [Required]
    public required string Description { get; set; }

    public float CaloriesBurned { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public UserInformation User { get; set; } = null!;
}
