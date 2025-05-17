using System.Text.Json.Serialization;

namespace NutriWave.API.Models.NutritionixApiModels;

public class FullNutrient
{
    [JsonPropertyName("attr_id")]
    public int AttrId { get; set; }

    [JsonPropertyName("value")]
    public float Value { get; set; }
}