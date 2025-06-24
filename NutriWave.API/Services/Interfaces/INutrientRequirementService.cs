using NutriWave.API.Models;
using NutriWave.API.Models.FileProcessingModels;

namespace NutriWave.API.Services.Interfaces;

public interface INutrientRequirementService
{
    Task AddUserNutrientRequirements(int userId, Sex sex, int age);

    Task<List<UserNutrientRequirement>> GetUserNutrientRequirements(int userId);

    Task RestoreAllNutrientRequirementsToDefault(int userId);

    Task UpdateNutrientRequirementForUserInBulk(int userId, List<NutrientChange> nutrientChanges);
}