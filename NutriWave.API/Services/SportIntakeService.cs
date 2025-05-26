using Microsoft.EntityFrameworkCore;
using NutriWave.API.Clients;
using NutriWave.API.Data;
using NutriWave.API.Models;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Services;

public class SportIntakeService(AppDbContext context, ISportLogService sportLogService, ICacheService cacheService, INutritionixClient nutritionixClient) : ISportIntakeService
{
    public async Task AddSportToUser(InfoRequest request)
    {
        var apiResponse = await nutritionixClient.GetSportInfoAsync(request.Description);
        var sportUsefulData = apiResponse.Exercises.Select(exercise => new SportUsefulData { Name = exercise.Name, CaloriesBurned = (float)exercise.Calories }).ToList();
        await cacheService.SaveSportInfo(request, sportUsefulData);

        var sportCaloriesSum = sportUsefulData.Sum(s => s.CaloriesBurned);
        await sportLogService.AddSportLog(request, sportCaloriesSum);

        var caloriesIntake = await GetUserCaloriesIntakeForToday(request.UserId);
        caloriesIntake.Quantity -= sportCaloriesSum;

        await context.SaveChangesAsync();
    }
    public async Task DeleteSport(InfoRequest request)
    {
        await sportLogService.DeleteSportLogForToday(request);

        var sportUsefulData = await cacheService.GetSportInfo(request);
        if(sportUsefulData is { Count: > 0 })
        {
            var caloriesIntake = await GetUserCaloriesIntakeForToday(request.UserId);
            caloriesIntake.Quantity += sportUsefulData.Sum(s => s.CaloriesBurned);

            await cacheService.RemoveSportInfo(request);
        }

        await context.SaveChangesAsync();
    }

    private async Task<UserNutrientIntake> GetUserCaloriesIntakeForToday(int userId)
    {
        return await context.UserNutrientIntakes.FirstAsync(n => n.UserId == userId && n.NutrientId == 1 && n.Date.Date == DateTime.Today);
    }
}
