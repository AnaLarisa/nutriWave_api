namespace NutriWave.API.Models.FileProcessingModels;

public class ProcessingResult
{
    public List<TestResult> TestResults { get; set; } = new();
    public int TotalResults { get; set; }
    public int AnonymizedImages { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = "";

    public List<object> NutrientRecommendations { get; set; } = new();
}