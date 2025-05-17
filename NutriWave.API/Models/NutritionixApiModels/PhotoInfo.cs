using System.Text.Json.Serialization;

namespace NutriWave.API.Models.NutritionixApiModels;

public class PhotoInfo
{
    [JsonPropertyName("thumb")]
    public string? Thumb { get; set; }

    [JsonPropertyName("highres")]
    public string? Highres { get; set; }

    [JsonPropertyName("is_user_uploaded")]
    public bool IsUserUploaded { get; set; }
}