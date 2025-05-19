using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class NutrientsController(INutrientIntakeService nutrientIntakeService) : ControllerBase
{

    [HttpPost("api/food-intake")]
    public async Task<IActionResult> UpdateFoodIntake([FromBody] GetInfoRequest request)
    {
        try
        {
            await nutrientIntakeService.UpdateNutrientIntakeAfterFood(request);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
        return StatusCode(StatusCodes.Status200OK, "The daily nutrients have been updated!");
    }

    [HttpDelete("api/food-intake")]
    public async Task<IActionResult> RemoveFoodIntake([FromBody] GetInfoRequest request)
    {
        try
        {
            await nutrientIntakeService.RemoveFoodIntake(request);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
        return StatusCode(StatusCodes.Status200OK, "The daily nutrients have been updated!");
    }
}

