using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services.Interfaces;

public interface ISportLogService
{
    Task AddSportLog(InfoRequest request, float calories);
    Task<List<SportLog>> GetSportLogs(int userId, DateTime date);
    Task DeleteSportLogForToday(InfoRequest request);
}