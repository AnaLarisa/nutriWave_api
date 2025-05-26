using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;
using System.Security.Claims;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SportController(ISportIntakeService sportIntakeService) : ControllerBase
{

    [HttpPost("api/sport-intake")]
    public async Task<IActionResult> UpdateSportIntake([FromBody] string description)
    {
        try
        {
            var infoRequest = new InfoRequest() { Description = description, UserId = UserId() };
            await sportIntakeService.AddSportToUser(infoRequest);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
        return StatusCode(StatusCodes.Status200OK, "The daily nutrients have been updated!");
    }

    [HttpDelete("api/sport-intake")]
    public async Task<IActionResult> RemoveSportIntake([FromBody] string description)
    {
        try
        {
            var infoRequest = new InfoRequest() { Description = description, UserId = UserId() };
            await sportIntakeService.DeleteSport(infoRequest);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
        return StatusCode(StatusCodes.Status200OK, "The daily nutrients have been updated!");
    }


    private int UserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
