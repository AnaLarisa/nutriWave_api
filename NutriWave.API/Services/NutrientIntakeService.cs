using Microsoft.EntityFrameworkCore;
using NutriWave.API.Clients;
using NutriWave.API.Data;
using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services;

public class NutrientIntakeService(AppDbContext context, INutritionixClient nutritionixClient) : INutrientIntakeService
{
    public async Task AddNewNutrientIntakeForTodayIfNotPresent(int userId)
    {
        var isIntakePresentForToday = context.UserNutrientIntakes
            .Any(u => u.UserId == userId && u.Date.Date == DateTime.Today);

        if (!isIntakePresentForToday)
        {
            var nutrientsForUserId = context.UserNutrientRequirements.Where(r => r.UserId == userId).Select(r => r.NutrientId).ToList();
            foreach (var n in nutrientsForUserId)
            {
                context.UserNutrientIntakes.Add(new UserNutrientIntake { Date = DateTime.Today, NutrientId = n, UserId = userId, Quantity = 0 });
            }
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<UserNutrientIntake>> GetNutrientIntakesForToday(int userId)
    {
        return await context.UserNutrientIntakes
            .Include(ni => ni.Nutrient)
            .Where(ni => ni.UserId == userId && ni.Date.Date == DateTime.Today)
            .ToListAsync();
    }

    public async Task UpdateNutrientIntakeAfterFood(FoodIntakeRequest request)
    {
        var response = await nutritionixClient.GetFoodInfoAsync(request.FoodDescription);
        if (response == null || response.Foods.Count == 0)
        {
            throw new Exception($"No food information found for {request.FoodDescription}.");
        }
        
        var totalNutrientValues = new Dictionary<int, float>();

        foreach (var nutrient in response.Foods.SelectMany(food => food.FullNutrients))
        {
            if (totalNutrientValues.ContainsKey(nutrient.AttrId))
                totalNutrientValues[nutrient.AttrId] += nutrient.Value;
            else
                totalNutrientValues[nutrient.AttrId] = nutrient.Value;
        }

        var userIntakes = await context.UserNutrientIntakes
            .Where(ni => ni.UserId == request.UserId && ni.Date == DateTime.Today)
            .ToDictionaryAsync(ni => ni.NutrientId);

        foreach (var (nutrientId, addedQuantity) in totalNutrientValues)
        {
            if (userIntakes.TryGetValue(nutrientId, out var intake))
            {
                intake.Quantity += addedQuantity;
            }
        }

        await context.SaveChangesAsync();
    }
}
