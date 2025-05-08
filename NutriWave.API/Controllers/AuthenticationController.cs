using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriWave.API.Models;
using NutriWave.API.Models.DTO;
using System.Text;
using NutriWave.API.Data;
using NutriWave.API.Services;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthenticationController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("CreateAccount")]
    public async Task<IActionResult> CreateAccount([FromBody] RegisterRequest request)
    {
        if (await _authService.UserExistsAsync(request.Email))
        {
            return BadRequest("User with this email already exists.");
        }

        await _authService.CreateUserAsync(request);
        return Ok(new { message = "Account created successfully." });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LogIn([FromBody] LoginRequest request)
    {
        var user = await _authService.AuthenticateUserAsync(request.Email, request.Password);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _authService.GenerateJwtToken(user);
        return Ok(new { token });
    }

}
