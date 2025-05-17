using System.Text.Json.Serialization;

namespace NutriWave.API.Models.NutritionixApiModels;

public class ExercisePhoto
{
    [JsonPropertyName("highres")]
    public string HighRes { get; set; }

    [JsonPropertyName("thumb")]
    public string Thumb { get; set; }

    [JsonPropertyName("is_user_uploaded")]
    public bool IsUserUploaded { get; set; }
}