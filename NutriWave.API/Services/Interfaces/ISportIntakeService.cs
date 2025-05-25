using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services.Interfaces;

public interface ISportIntakeService
{
    Task AddSportToUser(InfoRequest request);
    Task DeleteSport(InfoRequest request);
}