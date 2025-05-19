using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services;

public interface ICacheService
{
    Task SaveFoodNutrients(GetInfoRequest request, Dictionary<int, float> apiNutrients);
    Task<Dictionary<int, float>?> GetFoodNutrients(GetInfoRequest request);
    Task RemoveFoodNutrients(GetInfoRequest request);
}