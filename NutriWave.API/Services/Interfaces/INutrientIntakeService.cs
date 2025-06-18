using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services.Interfaces;

public interface INutrientIntakeService
{
    Task AddNewNutrientIntakeForTodayIfNotPresent(int userId);
    Task<List<UserNutrientIntake>> GetNutrientIntakesByDate(int userId, DateTime startDate, DateTime? endDate = null);
    Task UpdateNutrientIntakeAfterFood(InfoRequest request);
    Task RemoveFoodIntake(InfoRequest request);
}