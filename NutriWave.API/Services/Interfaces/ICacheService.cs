using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services.Interfaces;

public interface ICacheService
{
    Task SaveFoodNutrients(InfoRequest request, Dictionary<int, float> apiNutrients);
    Task<Dictionary<int, float>?> GetFoodNutrients(InfoRequest request);
    Task RemoveFoodNutrients(InfoRequest request);
    Task SaveSportInfo(InfoRequest request, IList<SportUsefulData> sportInfo);
    Task<IList<SportUsefulData>?> GetSportInfo(InfoRequest request);
    Task RemoveSportInfo(InfoRequest request);
}