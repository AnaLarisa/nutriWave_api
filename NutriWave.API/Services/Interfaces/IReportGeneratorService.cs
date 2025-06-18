using NutriWave.API.Models;

namespace NutriWave.API.Services.Interfaces;

public interface IReportGeneratorService
{
    Task<byte[]> GeneratePdfReportBytes(int userId, IEnumerable<UserNutrientIntake> nutrientIntakes, IEnumerable<FoodLog> foodLogs,
        DateTime startDate, DateTime endDate);

    Task<string> GenerateCsvReport(int userId, IEnumerable<UserNutrientIntake> nutrientIntakes,
        IEnumerable<FoodLog> foodLogs,
        DateTime startDate, DateTime endDate);

    string GenerateHl7Report(int userId, string firstName, string lastName, string email,
        IEnumerable<UserNutrientIntake> nutrientIntakes, IEnumerable<FoodLog> foodLogs,
        DateTime startDate, DateTime endDate);
}