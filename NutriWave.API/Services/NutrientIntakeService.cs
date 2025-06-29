﻿using Microsoft.EntityFrameworkCore;
using NutriWave.API.Clients;
using NutriWave.API.Data;
using NutriWave.API.Models;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Services;

public class NutrientIntakeService(AppDbContext context, ICacheService cacheService, INutritionixClient nutritionixClient, IFoodLogService foodLogService) : INutrientIntakeService
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

    public async Task<List<UserNutrientIntake>> GetNutrientIntakesByDate(int userId, DateTime startDate, DateTime? endDate = null)
    {
        if (endDate == null)
        {
            return await context.UserNutrientIntakes
                .Include(ni => ni.Nutrient)
                .Where(ni => ni.UserId == userId && ni.Date.Date == startDate)
                .ToListAsync();
        }

        return await context.UserNutrientIntakes
            .Include(ni => ni.Nutrient)
            .Where(ni => ni.UserId == userId && ni.Date.Date >= startDate && ni.Date.Date <= endDate)
            .OrderBy(ni => ni.Date)
            .ThenBy(ni => ni.Nutrient.Name)
            .ToListAsync();
    }

    public async Task UpdateNutrientIntakeAfterFood(InfoRequest request)
    {
        var response = await nutritionixClient.GetFoodInfoAsync(request.Description);
        if (response == null || response.Foods.Count == 0)
        {
            throw new Exception($"No food information found for {request.Description}.");
        }

        request.DisplayName = request.Description;
        
        var totalNutrientValues = new Dictionary<int, float>();

        foreach (var nutrient in response.Foods.SelectMany(food => food.FullNutrients))
        {
            if (totalNutrientValues.ContainsKey(nutrient.AttrId))
                totalNutrientValues[nutrient.AttrId] += nutrient.Value;
            else
                totalNutrientValues[nutrient.AttrId] = nutrient.Value;
        }

        await cacheService.SaveFoodNutrients(request, totalNutrientValues);
        await foodLogService.AddFoodIntakeRequestLog(request);

        var userIntakes = await GetUserIntakeForTodayToDictionaryAsync(request);

        foreach (var (nutrientId, addedQuantity) in totalNutrientValues)
        {
            if (userIntakes.TryGetValue(nutrientId, out var intake))
            {
                intake.Quantity += addedQuantity;
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<string> UpdateNutrientIntakeAfterBarcode(InfoRequest request)
    {
        var response = await nutritionixClient.GetBarcodeInfo(request.Description);
        if (response == null || response.Foods.Count == 0)
        {
            throw new ArgumentException($"No food information found for barcode {request.Description}.");
        }

        var food = response.Foods.First();
        var foodDisplayName = !string.IsNullOrEmpty(food.BrandName)
            ? $"{food.BrandName} {food.FoodName}"
            : food.FoodName;
        request.DisplayName = foodDisplayName;

        var totalNutrientValues = new Dictionary<int, float>();

        foreach (var nutrient in response.Foods.SelectMany(food => food.FullNutrients))
        {
            if (totalNutrientValues.ContainsKey(nutrient.AttrId))
                totalNutrientValues[nutrient.AttrId] += nutrient.Value;
            else
                totalNutrientValues[nutrient.AttrId] = nutrient.Value;
        }

        
        await cacheService.SaveFoodNutrients(request, totalNutrientValues);
        await foodLogService.AddFoodIntakeRequestLog(request);

        var userIntakes = await GetUserIntakeForTodayToDictionaryAsync(request);

        foreach (var (nutrientId, addedQuantity) in totalNutrientValues)
        {
            if (userIntakes.TryGetValue(nutrientId, out var intake))
            {
                intake.Quantity += addedQuantity;
            }
        }

        await context.SaveChangesAsync();

        return foodDisplayName;
    }

    public async Task RemoveFoodIntake(InfoRequest request)
    {
        var userIntakes = await GetUserIntakeForTodayToDictionaryAsync(request);

        var cachedNutrients = await cacheService.GetFoodNutrients(request);
        if (cachedNutrients == null)
        {
            throw new Exception("No cached intake data found for this food and user.");
        }

        foreach (var (nutrientId, quantityToSubtract) in cachedNutrients)
        {
            if (userIntakes.TryGetValue(nutrientId, out var intake))
            {
                intake.Quantity = Math.Max(0, intake.Quantity - quantityToSubtract);
            }
        }

        await cacheService.RemoveFoodNutrients(request);
        await foodLogService.DeleteFoodIntakeLogForToday(request);

        await context.SaveChangesAsync();
    }

    private async Task<Dictionary<int, UserNutrientIntake>> GetUserIntakeForTodayToDictionaryAsync(InfoRequest request)
    {
        return await context.UserNutrientIntakes
            .Where(ni => ni.UserId == request.UserId && ni.Date == DateTime.Today)
            .ToDictionaryAsync(ni => ni.NutrientId);
    }
}
