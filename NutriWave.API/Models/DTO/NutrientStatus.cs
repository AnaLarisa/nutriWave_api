namespace NutriWave.API.Models.DTO;

public class NutrientStatus
{
    public required string NutrientName { get; set; }

    public float DailyGoal { get; set; }

    public float CurrentIntake { get; set; }

    public float RemainingIntake => DailyGoal - CurrentIntake;

    public required string MeasuringUnit { get; set; }

}
