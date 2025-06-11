using NutriWave.API.Models.FileProcessingModels;

namespace NutriWave.API.Services.Interfaces;
public interface IMedicalPdfService
{
    Task<ProcessingResult> ProcessPdfAsync(byte[] pdfBytes, string filename);
}