using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;
using System.Security.Claims;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SportController(ISportIntakeService sportIntakeService, ISportLogService sportLogService) : ControllerBase
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

    [HttpGet("api/sport-logs")]
    public async Task<IActionResult> GetSportLogs(DateTime dateTime)
    {
        try
        {
            var userId = UserId();
            var sportLogs = await sportLogService.GetSportLogsByDate(userId, dateTime);
            return Ok(sportLogs);
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
