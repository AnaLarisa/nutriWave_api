using System.Text.Json.Serialization;

namespace NutriWave.API.Models.NutritionixApiModels;

public class ExerciseResponse
{
    [JsonPropertyName("exercises")]
    public List<Exercise> Exercises { get; set; }
}