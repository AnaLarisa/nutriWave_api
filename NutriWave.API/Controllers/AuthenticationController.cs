using Microsoft.AspNetCore.Mvc;
using NutriWave.API.Models.DTO;
using NutriWave.API.Services.Interfaces;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(IAuthService authService) : ControllerBase
{
    [HttpPost("CreateAccount")]
    public async Task<IActionResult> CreateAccount([FromBody] RegisterRequest request)
    {
        if (await authService.UserExistsAsync(request.Email))
        {
            return BadRequest("User with this email already exists.");
        }

        await authService.CreateUserAsync(request);
        return Ok(new { message = "Account created successfully." });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LogIn([FromBody] LoginRequest request)
    {
        var user = await authService.AuthenticateUserAsync(request.Email, request.Password);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = authService.GenerateJwtToken(user);
        return Ok(token);
    }

}
