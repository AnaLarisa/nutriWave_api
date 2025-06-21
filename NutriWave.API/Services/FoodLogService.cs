using Microsoft.EntityFrameworkCore;
using NutriWave.API.Data;
using NutriWave.API.Models;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Services;

public class FoodLogService(AppDbContext context) : IFoodLogService
{
    public async Task AddFoodIntakeRequestLog(InfoRequest foodRequest)
    {
        var foodLog = new FoodLog
        {
            UserId = foodRequest.UserId,
            Description = foodRequest.Description,
            DisplayName = foodRequest.DisplayName,
            Date = DateTime.Today
        };

        await context.FoodLogs.AddAsync(foodLog);
        await context.SaveChangesAsync();
    }

    public async Task DeleteFoodIntakeLogForToday(InfoRequest foodRequest)
    {
        var foodLog = await context.FoodLogs
            .FirstOrDefaultAsync(f =>
                f.DisplayName == foodRequest.Description &&
                f.Date.Date == DateTime.Today &&
                f.UserId == foodRequest.UserId);
        if (foodLog == null)
        {
            throw new Exception($"No food log found for {foodRequest.Description} on {DateTime.Today} for userId {foodRequest.UserId}.");
        }

        context.FoodLogs.Remove(foodLog);
        await context.SaveChangesAsync();
    }

    public async Task<List<FoodLog>> GetFoodLogsByDate(int userId, DateTime startDate, DateTime? endDate = null)
    {
        if (endDate == null)
        {
            return await context.FoodLogs
                .Where(log => log.UserId == userId && log.Date.Date == startDate.Date)
                .ToListAsync();
        }

        return await context.FoodLogs
            .Where(log => log.UserId == userId && log.Date.Date >= startDate.Date && log.Date.Date <= endDate.Value.Date)
            .OrderBy(log => log.Date)
            .ThenBy(log => log.Description)
            .ToListAsync();
    }
}
