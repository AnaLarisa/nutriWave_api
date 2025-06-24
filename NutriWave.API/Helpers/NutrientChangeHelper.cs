using System.Text.RegularExpressions;
using NutriWave.API.Models.FileProcessingModels;

namespace NutriWave.API.Helpers;

public class NutrientChangeHelper
{

    public static List<NutrientChange> ParseFromObjectList(List<object> objectList)
    {
        if (objectList == null)
            return new List<NutrientChange>();

        return objectList
            .Where(obj => obj != null)
            .Select(obj => obj.ToString())
            .Select(ParseRecommendationString)
            .Where(change => change != null)
            .ToList();
    }

    private static NutrientChange? ParseRecommendationString(string str)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            // Remove curly braces and split by comma
            var cleanStr = Regex.Replace(str, @"[{}]", "").Trim();
            var parts = cleanStr.Split(',');

            string nutrient = null;
            string dosageChange = null;

            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();

                    if (key.Equals("nutrient", StringComparison.OrdinalIgnoreCase))
                    {
                        nutrient = value;
                    }
                    else if (key.Equals("dosage_change", StringComparison.OrdinalIgnoreCase))
                    {
                        dosageChange = value;
                    }
                }
            }

            if (!string.IsNullOrEmpty(nutrient) && !string.IsNullOrEmpty(dosageChange))
            {
                return new NutrientChange
                {
                    Nutrient = nutrient,
                    DosageChange = dosageChange,
                    DbId = ApiMappingHelper.GetNutrientDbId(nutrient)
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing recommendation string: {str} - Error: {ex.Message}");
        }

        return null;
    }
}
