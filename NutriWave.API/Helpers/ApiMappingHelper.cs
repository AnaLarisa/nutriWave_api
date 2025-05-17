using NutriWave.API.Models.NutritionixApiModels;

namespace NutriWave.API.Helpers;

public static class ApiMappingHelper
{
    /// <summary>
    /// Based on Nutritionix API documentation: https://docx.syndigo.com/developers/docs/list-of-all-nutrients-and-nutrient-ids-from-api
    /// </summary>
    public static readonly Dictionary<int, int> AttrIdToDbId = new()
    {
        { 208, 1 },   // Energy (kcal)
        { 203, 2 },   // Protein (g)
        { 205, 3 },   // Carbohydrates (g)
        { 291, 4 },   // Fiber (g)
        { 204, 5 },   // Total Fat (g)
        { 606, 6 },   // Saturated Fat (g)
        { 645, 7 },   // Monounsaturated Fat (g)
        { 646, 8 },   // Polyunsaturated Fat (g)
        { 601, 9 },   // Cholesterol (mg)
        { 269, 10 },  // Sugars (g)
        { 539, 11 },  // Added Sugars (g) — fallback; Nutritionix may not always return this
        { 255, 12 },  // Water (mL)
        { 318, 13 },  // Vitamin A (µg)
        { 401, 14 },  // Vitamin C (mg)
        { 324, 15 },  // Vitamin D (µg)
        { 323, 16 },  // Vitamin E (mg)
        { 430, 17 },  // Vitamin K (µg)
        { 404, 18 },  // Thiamin (B1) (mg)
        { 405, 19 },  // Riboflavin (B2) (mg)
        { 406, 20 },  // Niacin (B3) (mg)
        { 415, 21 },  // Vitamin B6 (mg)
        { 435, 22 },  // Folate (B9) (µg)
        { 418, 23 },  // Vitamin B12 (µg)
        { 301, 24 },  // Calcium (mg)
        { 303, 25 },  // Iron (mg)
        { 304, 26 },  // Magnesium (mg)
        { 305, 27 },  // Phosphorus (mg)
        { 306, 28 },  // Potassium (mg)
        { 307, 29 },  // Sodium (mg)
        { 309, 30 },  // Zinc (mg)
        { 312, 31 },  // Copper (mg)
        { 315, 32 },  // Manganese (mg)
        { 317, 33 },  // Selenium (µg)
        { 636, 34 },  // Iodine (µg) — Note: use with caution, not always present
    };
    public static void ReplaceAttrIdsWithDbIds(NutritionixResponse response)
    {
        foreach (var nutritionixFood in response.Foods)
        {
            var fullNutrients = nutritionixFood.FullNutrients;

            foreach (var nutrient in fullNutrients)
            {
                nutrient.AttrId = AttrIdToDbId.GetValueOrDefault(nutrient.AttrId, -1);
            }

            nutritionixFood.FullNutrients = fullNutrients.Where(n => n is { AttrId: > 0, Value: > float.Epsilon }).ToList();
        }
    }




}

