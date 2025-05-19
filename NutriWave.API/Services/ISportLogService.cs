using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services;

public interface ISportLogService
{
    Task AddSportLog(GetInfoRequest request);
    Task<List<SportLog>> GetSportLogs(int userId, DateTime date);
    Task DeleteSportLogForToday(GetInfoRequest request);
}