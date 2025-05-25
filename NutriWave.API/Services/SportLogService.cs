using Microsoft.EntityFrameworkCore;
using NutriWave.API.Data;
using NutriWave.API.Models;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Services;

public class SportLogService(AppDbContext context) : ISportLogService
{
    public async Task AddSportLog(InfoRequest request)
    {
        var sportLog = new SportLog
        {
            UserId = request.UserId,
            Description = request.Description,
            Date = DateTime.Today
        };

        await context.SportLogs.AddAsync(sportLog);
        await context.SaveChangesAsync();
    }

    public async Task<List<SportLog>> GetSportLogs(int userId, DateTime date)
    {
        return await context.SportLogs
            .Where(log => log.UserId == userId && log.Date.Date == date.Date)
            .ToListAsync();
    }

    public async Task DeleteSportLogForToday(InfoRequest request)
    {
        var sportLog = await context.SportLogs
            .FirstOrDefaultAsync(s => s.Description == request.Description && s.Date.Date == DateTime.Today && s.UserId.Equals(request.UserId));

        if (sportLog == null)
        {
            throw new Exception($"No sport log found for '{request.Description}' on {DateTime.Today:yyyy-MM-dd} for userId {request.UserId}.");
        }

        context.SportLogs.Remove(sportLog);
        await context.SaveChangesAsync();
    }
}
