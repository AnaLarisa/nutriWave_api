using NutriWave.API.Helpers;
using System.Text.Json.Serialization;

namespace NutriWave.API.Models.FileProcessingModels;

public class NutrientChange
{
    [JsonPropertyName("nutrient")]
    public required string Nutrient { get; set; }

    [JsonPropertyName("dosage_change")]
    public required string DosageChange { get; set; }

    [JsonPropertyName("db_id")]
    public int? DbId { get; set; }

    [JsonIgnore]
    public bool ShouldIncrease => DosageChange.Trim() == "+";

    [JsonIgnore]
    public bool ShouldDecrease => DosageChange.Trim() == "-";

    public NutrientChange()
    {
    }

    public NutrientChange(string nutrient, string dosageChange)
    {
        Nutrient = nutrient;
        DosageChange = dosageChange;
        DbId = ApiMappingHelper.GetNutrientDbId(nutrient);
    }
}
