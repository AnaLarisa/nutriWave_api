using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Helpers;

public static class DtoMappingHelper
{
    public static IList<NutrientStatus> GetFullNutrientStatusList(IList<UserNutrientIntake> nutrientIntakes,
        IList<UserNutrientRequirement> nutrientRequirements)
    {
        var list = new List<NutrientStatus>();
        foreach (var requirement in nutrientRequirements)
        {
            var intake = nutrientIntakes.FirstOrDefault(i => i.NutrientId == requirement.NutrientId);
            var status = new NutrientStatus
            {
                NutrientName = requirement.Nutrient.Name,
                DailyGoal = requirement.Quantity,
                CurrentIntake = intake?.Quantity ?? 0,
                MeasuringUnit = requirement.Nutrient.Unit
            };

            list.Add(status);
        }
        return list;
    }
}
