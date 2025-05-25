using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services.Interfaces;

public interface INutrientIntakeService
{
    Task AddNewNutrientIntakeForTodayIfNotPresent(int userId);
    Task<List<UserNutrientIntake>> GetNutrientIntakesForToday(int userId);
    Task UpdateNutrientIntakeAfterFood(InfoRequest request);
    Task RemoveFoodIntake(InfoRequest request);
}