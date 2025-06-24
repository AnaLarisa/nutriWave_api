using Microsoft.EntityFrameworkCore;
using NutriWave.API.Data;
using NutriWave.API.Helpers;
using NutriWave.API.Models;
using NutriWave.API.Models.FileProcessingModels;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Services;

public class NutrientRequirementService(AppDbContext context) : INutrientRequirementService
{
    // Recommended daily nutrient intakes based on European Food Safety Authority (EFSA) guidelines.
    private float GetRecommendedQuantity(int nutrientId, Sex sex, int age)
    {
        return nutrientId switch
        {
            1 => 2500f,                                        // Energy (kcal)
            2 => sex == Sex.Male ? 56f : 46f,                  // Protein (g)
            3 => 130f,                                         // Carbohydrates (g)
            4 => age < 18 ? 25f : 30f,                         // Fiber (g)
            5 => 70f,                                          // Total Fat (g)
            6 => 20f,                                          // Saturated Fat (g)
            7 => 20f,                                          // Monounsaturated Fat (g)
            8 => 17f,                                          // Polyunsaturated Fat (g)
            9 => 300f,                                         // Cholesterol (mg)
            10 => 50f,                                          // Sugars (g)
            11 => 25f,                                          // Added Sugars (g)
            12 => 2000f,                                        // Water (mL)
            13 => 900f,                                         // Vitamin A (µg)
            14 => sex == Sex.Male ? 90f : 75f,                  // Vitamin C (mg)
            15 => 15f,                                          // Vitamin D (µg)
            16 => 15f,                                          // Vitamin E (mg)
            17 => 120f,                                         // Vitamin K (µg)
            18 => 1.2f,                                         // Thiamin (B1) (mg)
            19 => 1.3f,                                         // Riboflavin (B2) (mg)
            20 => 16f,                                          // Niacin (B3) (mg)
            21 => 1.3f,                                         // Vitamin B6 (mg)
            22 => 400f,                                         // Folate (B9) (µg)
            23 => 2.4f,                                         // Vitamin B12 (µg)
            24 => age < 18 ? 1300f : 1000f,                     // Calcium (mg)
            25 => sex == Sex.Female && age >= 19 && age <= 50
                    ? 18f : 8f,                                 // Iron (mg)
            26 => 400f,                                         // Magnesium (mg)
            27 => 700f,                                         // Phosphorus (mg)
            28 => 4700f,                                        // Potassium (mg)
            29 => 1500f,                                        // Sodium (mg)
            30 => 11f,                                          // Zinc (mg)
            31 => 0.9f,                                         // Copper (mg)
            32 => 2.3f,                                         // Manganese (mg)
            33 => 55f,                                          // Selenium (µg)
            34 => 150f,                                         // Iodine (µg)
            _ => 0f
        };
    }

    public async Task AddUserNutrientRequirements(int userId, Sex sex, int age)
    {
        var requirements = Enumerable.Range(1, 34).Select(i => new UserNutrientRequirement
        {
            UserId = userId,
            NutrientId = i,
            Quantity = GetRecommendedQuantity(i, sex, age)
        }).ToList();

        context.UserNutrientRequirements.AddRange(requirements);
        await context.SaveChangesAsync();
    }

    public Task<List<UserNutrientRequirement>> GetUserNutrientRequirements(int userId)
    {
        return context.UserNutrientRequirements
            .Where(r => r.UserId == userId)
            .Include(r => r.Nutrient)
            .ToListAsync();
    }

    public async Task RestoreAllNutrientRequirementsToDefault(int userId)
    {

        var existingRequirements = await context.UserNutrientRequirements
            .Where(req => req.UserId == userId)
            .Include(req => req.User)
            .ToListAsync();
        var sex = (Sex)existingRequirements.FirstOrDefault()?.User?.Sex;
        var age = CalculationsHelper.CalculateAge((DateTime)existingRequirements.FirstOrDefault()?.User.BirthDate);

        foreach (var requirement in existingRequirements)
        {
            requirement.Quantity = GetRecommendedQuantity(requirement.NutrientId, sex, age);
        }

        await context.SaveChangesAsync();
    }

    public async Task UpdateNutrientRequirementForUserInBulk(int userId, List<NutrientChange> nutrientChanges)
    {
        var validChanges = nutrientChanges.Where(nc => nc.DbId.HasValue).ToList();

        if (!validChanges.Any())
        {
            return;
        }
        var nutrientIds = validChanges.Select(nc => nc.DbId.Value).ToList();

        var existingRequirements = await context.UserNutrientRequirements
            .Where(req => req.UserId == userId && nutrientIds.Contains(req.NutrientId))
            .ToListAsync();


        var defaultRequirements = GetDefaultNutrientRequirements();

        foreach (var change in validChanges)
        {
            var nutrientId = change.DbId.Value;
            var existingRequirement = existingRequirements.FirstOrDefault(req => req.NutrientId == nutrientId);

            var newQuantity = CalculateNewQuantity(
                existingRequirement.Quantity,
                change.ShouldIncrease,
                nutrientId
            );

            existingRequirement.Quantity = ApplySafetyLimits(newQuantity, nutrientId);
        }

        await context.SaveChangesAsync();
    }

    private float ApplySafetyLimits(float quantity, int nutrientId)
    {
        var (minSafe, maxSafe) = GetSafetyLimits(nutrientId);

        // Ensure quantity is within safe range
        if (quantity < minSafe) return minSafe;
        if (quantity > maxSafe) return maxSafe;

        return quantity;
    }

    /// <summary>
    /// Gets minimum and maximum safe daily amounts for each nutrient
    /// Based on RDA (Recommended Dietary Allowance) and UL (Upper Limit)
    /// </summary>
    private (float min, float max) GetSafetyLimits(int nutrientId)
    {
        return nutrientId switch
        {
            // Macronutrients
            1 => (1200f, 4000f),    // Energy (kcal) - 1200-4000
            2 => (10f, 200f),       // Protein (g) - 10-200
            3 => (50f, 500f),       // Carbohydrates (g) - 50-500
            4 => (10f, 50f),        // Fiber (g) - 10-50
            5 => (20f, 150f),       // Total Fat (g) - 20-150

            // Water-soluble vitamins
            14 => (30f, 2000f),     // Vitamin C (mg) - 30-2000
            18 => (0.5f, 50f),      // Thiamin B1 (mg) - 0.5-50
            19 => (0.6f, 50f),      // Riboflavin B2 (mg) - 0.6-50
            20 => (6f, 35f),        // Niacin B3 (mg) - 6-35
            21 => (0.5f, 100f),     // Vitamin B6 (mg) - 0.5-100
            22 => (150f, 1000f),    // Folate B9 (µg) - 150-1000
            23 => (1f, 3000f),      // Vitamin B12 (µg) - 1-3000

            // Fat-soluble vitamins
            13 => (300f, 3000f),    // Vitamin A (µg) - 300-3000
            15 => (5f, 100f),       // Vitamin D (µg) - 5-100
            16 => (6f, 1000f),      // Vitamin E (mg) - 6-1000
            17 => (30f, 1000f),     // Vitamin K (µg) - 30-1000

            // Minerals
            24 => (400f, 2500f),    // Calcium (mg) - 400-2500
            25 => (5f, 45f),        // Iron (mg) - 5-45
            26 => (150f, 400f),     // Magnesium (mg) - 150-400
            27 => (400f, 4000f),    // Phosphorus (mg) - 400-4000
            28 => (1600f, 4700f),   // Potassium (mg) - 1600-4700
            29 => (500f, 2300f),    // Sodium (mg) - 500-2300
            30 => (3f, 40f),        // Zinc (mg) - 3-40
            31 => (0.4f, 10f),      // Copper (mg) - 0.4-10
            32 => (1f, 11f),        // Manganese (mg) - 1-11
            33 => (20f, 400f),      // Selenium (µg) - 20-400
            34 => (70f, 1100f),     // Iodine (µg) - 70-1100

            // Default safety range
            _ => (0.1f, 1000f)
        };
    }


    private Dictionary<int, float> GetDefaultNutrientRequirements()
    {
        return new Dictionary<int, float>
        {
            // Macronutrients
            { 1, 2000f },    // Energy (kcal)
            { 2, 50f },      // Protein (g)
            { 3, 300f },     // Carbohydrates (g)
            { 4, 25f },      // Fiber (g)
            { 5, 65f },      // Total Fat (g)
            { 6, 20f },      // Saturated Fat (g)
            { 12, 2500f },   // Water (mL)
            
            // Vitamins
            { 13, 800f },    // Vitamin A (µg)
            { 14, 80f },     // Vitamin C (mg)
            { 15, 15f },     // Vitamin D (µg)
            { 16, 12f },     // Vitamin E (mg)
            { 17, 75f },     // Vitamin K (µg)
            { 18, 1.1f },    // Thiamin B1 (mg)
            { 19, 1.3f },    // Riboflavin B2 (mg)
            { 20, 15f },     // Niacin B3 (mg)
            { 21, 1.4f },    // Vitamin B6 (mg)
            { 22, 400f },    // Folate B9 (µg)
            { 23, 2.4f },    // Vitamin B12 (µg)
            
            // Minerals
            { 24, 1000f },   // Calcium (mg)
            { 25, 14f },     // Iron (mg)
            { 26, 300f },    // Magnesium (mg)
            { 27, 700f },    // Phosphorus (mg)
            { 28, 3500f },   // Potassium (mg)
            { 29, 1500f },   // Sodium (mg)
            { 30, 10f },     // Zinc (mg)
            { 31, 1f },      // Copper (mg)
            { 32, 2f },      // Manganese (mg)
            { 33, 55f },     // Selenium (µg)
            { 34, 150f }     // Iodine (µg)
        };
    }
    
    private float CalculateNewQuantity(float currentQuantity, bool shouldIncrease, int nutrientId)
    {
        var adjustmentPercentage = GetAdjustmentPercentage(nutrientId);

        if (shouldIncrease)
        {
            return currentQuantity * (1 + adjustmentPercentage);
        }

        return currentQuantity * (1 - adjustmentPercentage);
    }

    private float GetAdjustmentPercentage(int nutrientId)
    {
        return nutrientId switch
        {
            // Macronutrients - smaller adjustments (5-15%)
            1 => 0.10f,  // Energy - 10%
            2 => 0.15f,  // Protein - 15%
            3 => 0.10f,  // Carbohydrates - 10%
            5 => 0.10f,  // Total Fat - 10%

            // Water-soluble vitamins - can be adjusted more (15-25%)
            14 => 0.20f, // Vitamin C - 20%
            18 => 0.15f, // Thiamin (B1) - 15%
            19 => 0.15f, // Riboflavin (B2) - 15%
            20 => 0.15f, // Niacin (B3) - 15%
            21 => 0.15f, // Vitamin B6 - 15%
            22 => 0.25f, // Folate (B9) - 25%
            23 => 0.25f, // Vitamin B12 - 25%

            // Fat-soluble vitamins - smaller adjustments (10-15%)
            13 => 0.15f, // Vitamin A - 15%
            15 => 0.20f, // Vitamin D - 20%
            16 => 0.15f, // Vitamin E - 15%
            17 => 0.15f, // Vitamin K - 15%

            // Minerals - moderate adjustments (15-20%)
            24 => 0.15f, // Calcium - 15%
            25 => 0.20f, // Iron - 20%
            26 => 0.15f, // Magnesium - 15%
            30 => 0.20f, // Zinc - 20%

            // Other nutrients - default 15%
            _ => 0.15f
        };
    }
}
