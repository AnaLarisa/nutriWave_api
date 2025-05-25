using Microsoft.EntityFrameworkCore;
using NutriWave.API.Data;
using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services.Interfaces;

public class FoodLogService(AppDbContext context) : IFoodLogService
{
    public async Task AddFoodIntakeRequestLog(InfoRequest foodRequest)
    {
        var foodLog = new FoodLog
        {
            UserId = foodRequest.UserId,
            Description = foodRequest.Description,
            Date = DateTime.Today
        };

        await context.FoodLogs.AddAsync(foodLog);
        await context.SaveChangesAsync();
    }

    public async Task<List<FoodLog>> GetFoodIntakeLogs(int userId, DateTime date)
    {
        return await context.FoodLogs
            .Where(log => log.UserId == userId && log.Date.Date == date.Date)
            .ToListAsync();
    }

    public async Task DeleteFoodIntakeLogForToday(InfoRequest foodRequest)
    {
        var foodLog = await context.FoodLogs
            .FirstOrDefaultAsync(f =>
                f.Description == foodRequest.Description &&
                f.Date.Date == DateTime.Today &&
                f.UserId == foodRequest.UserId);
        if (foodLog == null)
        {
            throw new Exception($"No food log found for {foodRequest.Description} on {DateTime.Today} for userId {foodRequest.UserId}.");
        }

        context.FoodLogs.Remove(foodLog);
        await context.SaveChangesAsync();
    }

}
