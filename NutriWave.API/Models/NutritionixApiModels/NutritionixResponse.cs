using System.Text.Json.Serialization;

namespace NutriWave.API.Models.NutritionixApiModels;

public class NutritionixResponse
{
    [JsonPropertyName("foods")]
    public List<NutritionixFood> Foods { get; set; } = new();
}
