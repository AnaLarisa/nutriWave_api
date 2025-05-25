using NutriWave.API.Models.NutritionixApiModels;

namespace NutriWave.API.Clients;

public interface INutritionixClient
{
    Task<NutritionixResponse?> GetFoodInfoAsync(string food);
    Task<NutritionixResponse> GetBarcodeInfo(string barcodeId);
    Task<ExerciseResponse?> GetSportInfoAsync(string sport);
}