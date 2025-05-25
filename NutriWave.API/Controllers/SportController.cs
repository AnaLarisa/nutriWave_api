using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SportController(ISportIntakeService sportIntakeService) : ControllerBase
{

    [HttpPost("api/sport-intake")]
    public async Task<IActionResult> UpdateFoodIntake([FromBody] InfoRequest request)
    {
        try
        {
            await sportIntakeService.AddSportToUser(request);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
        return StatusCode(StatusCodes.Status200OK, "The daily nutrients have been updated!");
    }

    [HttpDelete("api/sport-intake")]
    public async Task<IActionResult> RemoveFoodIntake([FromBody] InfoRequest request)
    {
        try
        {
            await sportIntakeService.DeleteSport(request);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
        return StatusCode(StatusCodes.Status200OK, "The daily nutrients have been updated!");
    }




}
