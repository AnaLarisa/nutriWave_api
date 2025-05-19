using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services;
public interface IFoodLogService
{
    Task AddFoodIntakeRequestLog(GetInfoRequest foodRequest);
    Task DeleteFoodIntakeLogForToday(GetInfoRequest foodRequest);
    Task<List<FoodLog>> GetFoodIntakeLogs(int userId, DateTime date);
}