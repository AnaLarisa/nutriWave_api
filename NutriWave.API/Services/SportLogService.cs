using Microsoft.EntityFrameworkCore;
using NutriWave.API.Data;
using NutriWave.API.Models;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Services;

public class SportLogService(AppDbContext context) : ISportLogService
{
    public async Task AddSportLog(InfoRequest request, float caloriesBurned)
    {
        var sportLog = new SportLog
        {
            UserId = request.UserId,
            Description = request.Description,
            CaloriesBurned = caloriesBurned,
            Date = DateTime.Today
        };

        await context.SportLogs.AddAsync(sportLog);
        await context.SaveChangesAsync();
    }

    public async Task DeleteSportLogForToday(InfoRequest request)
    {
        var sportLog = await context.SportLogs
            .Where(s => s.Description == request.Description && s.Date.Date == DateTime.Today && s.UserId.Equals(request.UserId))
            .ToListAsync();

        if (sportLog == null)
        {
            throw new Exception($"No sport log found for '{request.Description}' on {DateTime.Today:yyyy-MM-dd} for userId {request.UserId}.");
        }

        context.SportLogs.RemoveRange(sportLog);
        await context.SaveChangesAsync();
    }

    public async Task<IList<SportLogDto>> GetSportLogsByDate(int userId, DateTime dateTime)
    {
        return await context.SportLogs
            .Where(log => log.UserId == userId && log.Date.Date == dateTime.Date)
            .Select(log => new SportLogDto
            {
                Description = log.Description,
                CaloriesBurned = log.CaloriesBurned,
            })
            .ToListAsync();
    }
}
