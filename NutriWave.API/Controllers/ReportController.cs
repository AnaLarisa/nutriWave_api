using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriWave.API.Services.Interfaces;
using System.Security.Claims;
using System.Text;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController(INutrientIntakeService nutrientIntakeService, IFoodLogService foodLogService, IReportGeneratorService reportGeneratorService) : ControllerBase
{
    /// <summary>
    /// Generates a PDF report of nutrient intakes and food logs for the specified date range
    /// </summary>
    [HttpGet("pdf")]
    public async Task<IActionResult> GeneratePdfReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var userId = UserId();
            var nutrientIntakes = await nutrientIntakeService.GetNutrientIntakesByDate(userId, startDate, endDate);
            var foodLogs = await foodLogService.GetFoodLogsByDate(userId, startDate, endDate);

            var excel  = await reportGeneratorService.GeneratePdfReportBytes(userId, nutrientIntakes, foodLogs, startDate, endDate);
            var fileName = $"NutrientReport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";
            return File(excel, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating PDF report: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a CSV report of nutrient intakes and food logs for the specified date range
    /// </summary>
    [HttpGet("csv")]
    public async Task<IActionResult> GenerateCsvReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var userId = UserId();
            var nutrientIntakes = await nutrientIntakeService.GetNutrientIntakesByDate(userId, startDate, endDate);
            var foodLogs = await foodLogService.GetFoodLogsByDate(userId, startDate, endDate);

            var csv = await reportGeneratorService.GenerateCsvReport(userId, nutrientIntakes, foodLogs, startDate, endDate);

            var fileName = $"NutrientReport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating CSV report: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates an HL7 FHIR-compatible report of nutrient intakes for the specified date range
    /// </summary>
    [HttpGet("hl7")]
    public async Task<IActionResult> GenerateHl7Report([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var userId = UserId();
            var nutrientIntakes = await nutrientIntakeService.GetNutrientIntakesByDate(userId, startDate, endDate);
            var foodLogs = await foodLogService.GetFoodLogsByDate(userId, startDate, endDate);

            var user = nutrientIntakes.FirstOrDefault()?.User;
            var firstName = user?.FirstName ?? "";
            var lastName = user?.LastName ?? "";
            var email = user?.Email ?? "";

            var hl7Content = reportGeneratorService.GenerateHl7Report(userId, firstName, lastName, email,
                nutrientIntakes, foodLogs, startDate, endDate);

            var fileName = $"NutrientReport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.hl7";
            var bytes = Encoding.UTF8.GetBytes(hl7Content);

            return File(bytes, "text/plain", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating HL7 report: {ex.Message}");
        }
    }

    private int UserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
