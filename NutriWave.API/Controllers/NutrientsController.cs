using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriWave.API.Helpers;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;
using System.Security.Claims;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class NutrientsController(INutrientIntakeService nutrientIntakeService, INutrientRequirementService nutrientRequirementService, IFoodLogService foodLogService) : ControllerBase
{

    [HttpGet("allNutrition")]
    public async Task<IActionResult> GetAllNutrition(DateTime dateTime)
    {
        try
        {
            var userId = UserId();
            var todayUserIntakes = await nutrientIntakeService.GetNutrientIntakesByDate(userId, dateTime);
            var userRequirements = await nutrientRequirementService.GetUserNutrientRequirements(userId);
            var nutrition = DtoMappingHelper.GetFullNutrientStatusList(todayUserIntakes, userRequirements);

            return Ok(nutrition);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }


    [HttpPost("food-intake")]
    public async Task<IActionResult> UpdateFoodIntake([FromBody] string description)
    {
        try
        {
            var infoRequest = new InfoRequest() { Description = description, UserId = UserId() };
            await nutrientIntakeService.UpdateNutrientIntakeAfterFood(infoRequest);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
        return StatusCode(StatusCodes.Status200OK, "The daily nutrients have been updated!");
    }

    [HttpDelete("food-intake")]
    public async Task<IActionResult> RemoveFoodIntake([FromBody] string description)
    {
        try
        {
            var infoRequest = new InfoRequest() { Description = description, UserId = UserId() };
            await nutrientIntakeService.RemoveFoodIntake(infoRequest);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
        return StatusCode(StatusCodes.Status200OK, "The daily nutrients have been updated!");
    }

    [HttpGet("food-logs")]
    public async Task<IActionResult> GetFoodLogs(DateTime dateTime)
    {
        try
        {
            var userId = UserId();
            var foodLogs = await foodLogService.GetFoodLogsByDate(userId, dateTime);

            return Ok(foodLogs.Select(f => f.Description));
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    private int UserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

}

