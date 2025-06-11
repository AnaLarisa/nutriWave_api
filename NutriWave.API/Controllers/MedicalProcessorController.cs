using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriWave.API.Services;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MedicalProcessorController : ControllerBase
{
    private readonly IMedicalPdfService _service;

    public MedicalProcessorController(IMedicalPdfService service)
    {
        _service = service;
    }

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

            var result = await _service.ProcessPdfAsync(pdfBytes, pdfFile.FileName);

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
}