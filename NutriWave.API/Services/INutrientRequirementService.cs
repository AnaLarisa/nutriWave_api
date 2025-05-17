using NutriWave.API.Models;

namespace NutriWave.API.Services;

public interface INutrientRequirementService
{
    Task AddUserNutrientRequirements(int userId, Sex sex, int age);
}