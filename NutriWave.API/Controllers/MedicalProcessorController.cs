using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriWave.API.Services.Interfaces;
using System.Security.Claims;

namespace NutriWave.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MedicalProcessorController(
    IMedicalPdfService service,
    INutrientRequirementService nutrientRequirementService)
    : ControllerBase
{
    [HttpPost("process-pdf")]
    public async Task<IActionResult> ProcessPdf(IFormFile pdfFile)
    {
        if (pdfFile == null || pdfFile.Length == 0)
        {
            return BadRequest("No PDF file provided");
        }

        if (!pdfFile.FileName.ToLower().EndsWith(".pdf"))
        {
            return BadRequest("File must be a PDF");
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await pdfFile.CopyToAsync(memoryStream);
            var pdfBytes = memoryStream.ToArray();

            var result = await service.ProcessPdfAsync(pdfBytes, pdfFile.FileName, UserId());

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, new { error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("restore-to-default")]
    public async Task<IActionResult> RestoreToDefault()
    {
        try
        {
            await nutrientRequirementService.RestoreAllNutrientRequirementsToDefault(UserId());
            return Ok(new { message = "Nutrient requirements restored to default." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }


    private int UserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}