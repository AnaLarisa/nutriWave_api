namespace NutriWave.API.Models.FileProcessingModels;

public class AnonymizationResult
{
    public string ImagePath { get; set; } = "";
    public bool WasAnonymized { get; set; }
    public string Provider { get; set; } = "";
}