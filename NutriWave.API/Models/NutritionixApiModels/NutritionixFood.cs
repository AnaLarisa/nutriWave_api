using System.Text.Json;
using System.Text.Json.Serialization;
namespace NutriWave.API.Models.NutritionixApiModels;

public class NutritionixFood
{
    [JsonPropertyName("food_name")]
    public string FoodName { get; set; } = string.Empty;

    [JsonPropertyName("brand_name")]
    public string? BrandName { get; set; }

    [JsonPropertyName("serving_qty")]
    public float ServingQty { get; set; }

    [JsonPropertyName("serving_unit")]
    public string ServingUnit { get; set; } = string.Empty;

    [JsonPropertyName("serving_weight_grams")]
    public float? ServingWeightGrams { get; set; }

    [JsonPropertyName("nf_metric_qty")]
    public float? NfMetricQty { get; set; }

    [JsonPropertyName("nf_metric_uom")]
    public string? NfMetricUom { get; set; }

    [JsonPropertyName("nf_calories")]
    public float? NfCalories { get; set; }

    [JsonPropertyName("nf_total_fat")]
    public float? NfTotalFat { get; set; }

    [JsonPropertyName("nf_saturated_fat")]
    public float? NfSaturatedFat { get; set; }

    [JsonPropertyName("nf_cholesterol")]
    public float? NfCholesterol { get; set; }

    [JsonPropertyName("nf_sodium")]
    public float? NfSodium { get; set; }

    [JsonPropertyName("nf_total_carbohydrate")]
    public float? NfTotalCarbohydrate { get; set; }

    [JsonPropertyName("nf_dietary_fiber")]
    public float? NfDietaryFiber { get; set; }

    [JsonPropertyName("nf_sugars")]
    public float? NfSugars { get; set; }

    [JsonPropertyName("nf_protein")]
    public float? NfProtein { get; set; }

    [JsonPropertyName("nf_potassium")]
    public float? NfPotassium { get; set; }

    [JsonPropertyName("nf_p")]
    public float? NfP { get; set; }

    [JsonPropertyName("full_nutrients")]
    public List<FullNutrient> FullNutrients { get; set; } = new();

    [JsonPropertyName("nix_brand_name")]
    public string? NixBrandName { get; set; }

    [JsonPropertyName("nix_brand_id")]
    public string? NixBrandId { get; set; }

    [JsonPropertyName("nix_item_name")]
    public string? NixItemName { get; set; }

    [JsonPropertyName("nix_item_id")]
    public string? NixItemId { get; set; }

    [JsonPropertyName("metadata")]
    public JsonElement Metadata { get; set; }

    [JsonPropertyName("source")]
    public int? Source { get; set; }

    [JsonPropertyName("lat")]
    public float? Lat { get; set; }

    [JsonPropertyName("lng")]
    public float? Lng { get; set; }

    [JsonPropertyName("photo")]
    public PhotoInfo? Photo { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("class_code")]
    public string? ClassCode { get; set; }

    [JsonPropertyName("brick_code")]
    public string? BrickCode { get; set; }

    [JsonPropertyName("tag_id")]
    public string? TagId { get; set; }

    [JsonPropertyName("nf_ingredient_statement")]
    public string? NfIngredientStatement { get; set; }

    [JsonPropertyName("consumed_at")]
    public DateTime? ConsumedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("alt_measures")]
    public JsonElement AltMeasures { get; set; }
}