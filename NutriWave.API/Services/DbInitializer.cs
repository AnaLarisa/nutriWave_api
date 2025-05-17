using NutriWave.API.Data;
using NutriWave.API.Models;

namespace NutriWave.API.Services;

public static class DbInitializer
{
    public static void SeedDatabase(AppDbContext context)
    {
        if (context.Nutrients.Any()) return; // Avoid duplicate seeding

        var nutrients = new List<Nutrient>
        {
            // Macronutrients
            new() { Name = "Energy", Unit = "kcal" },
            new() { Name = "Protein", Unit = "g" },
            new() { Name = "Carbohydrates", Unit = "g" },
            new() { Name = "Fiber", Unit = "g" },
            new() { Name = "Total Fat", Unit = "g" },
            new() { Name = "Saturated Fat", Unit = "g" },
            new() { Name = "Monounsaturated Fat", Unit = "g" },
            new() { Name = "Polyunsaturated Fat", Unit = "g" },
            new() { Name = "Cholesterol", Unit = "mg" },
            new() { Name = "Sugars", Unit = "g" },
            new() { Name = "Added Sugars", Unit = "g" },
            new() { Name = "Water", Unit = "mL" },

            // Selected Micronutrients (DRI-based)
            new() { Name = "Vitamin A", Unit = "µg" },
            new() { Name = "Vitamin C", Unit = "mg" },
            new() { Name = "Vitamin D", Unit = "µg" },
            new() { Name = "Vitamin E", Unit = "mg" },
            new() { Name = "Vitamin K", Unit = "µg" },
            new() { Name = "Thiamin (B1)", Unit = "mg" },
            new() { Name = "Riboflavin (B2)", Unit = "mg" },
            new() { Name = "Niacin (B3)", Unit = "mg" },
            new() { Name = "Vitamin B6", Unit = "mg" },
            new() { Name = "Folate (B9)", Unit = "µg" },
            new() { Name = "Vitamin B12", Unit = "µg" },
            new() { Name = "Calcium", Unit = "mg" },
            new() { Name = "Iron", Unit = "mg" },
            new() { Name = "Magnesium", Unit = "mg" },
            new() { Name = "Phosphorus", Unit = "mg" },
            new() { Name = "Potassium", Unit = "mg" },
            new() { Name = "Sodium", Unit = "mg" },
            new() { Name = "Zinc", Unit = "mg" },
            new() { Name = "Copper", Unit = "mg" },
            new() { Name = "Manganese", Unit = "mg" },
            new() { Name = "Selenium", Unit = "µg" },
            new() { Name = "Iodine", Unit = "µg" }
        };

        context.Nutrients.AddRange(nutrients);
        context.SaveChanges();
    }
}
