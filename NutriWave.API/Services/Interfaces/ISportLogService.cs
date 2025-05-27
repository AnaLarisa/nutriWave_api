using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services.Interfaces;

public interface ISportLogService
{
    Task AddSportLog(InfoRequest request, float calories);
    Task DeleteSportLogForToday(InfoRequest request);
    Task<IList<SportLogDto>> GetSportLogsByDate(int userId, DateTime dateTime);
}