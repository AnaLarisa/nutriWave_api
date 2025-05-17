using System.Text.Json.Serialization;

namespace NutriWave.API.Models.NutritionixApiModels;

public class Exercise
{
    [JsonPropertyName("tag_id")]
    public int TagId { get; set; }

    [JsonPropertyName("user_input")]
    public string UserInput { get; set; }

    [JsonPropertyName("duration_min")]
    public double DurationMin { get; set; }

    [JsonPropertyName("met")]
    public double Met { get; set; }

    [JsonPropertyName("nf_calories")]
    public double Calories { get; set; }

    [JsonPropertyName("photo")]
    public ExercisePhoto Photo { get; set; }

    [JsonPropertyName("compendium_code")]
    public int CompendiumCode { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("benefits")]
    public string Benefits { get; set; }
}