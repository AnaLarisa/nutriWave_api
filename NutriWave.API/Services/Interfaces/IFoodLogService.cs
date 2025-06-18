using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services.Interfaces;
public interface IFoodLogService
{
    Task AddFoodIntakeRequestLog(InfoRequest foodRequest);
    Task DeleteFoodIntakeLogForToday(InfoRequest foodRequest);
    Task<List<FoodLog>> GetFoodLogsByDate(int userId, DateTime startDate, DateTime? endDate = null);
}