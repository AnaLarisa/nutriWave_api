using NutriWave.API.Data;
using NutriWave.API.Models;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Services;

public class NutrientRequirementService : INutrientRequirementService
{
    private readonly AppDbContext _context;
    public NutrientRequirementService(AppDbContext context)
    {
        _context = context;
    }

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

        _context.UserNutrientRequirements.AddRange(requirements);
        await _context.SaveChangesAsync();
    }

}
