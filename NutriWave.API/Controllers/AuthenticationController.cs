using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriWave.API.Models;
using NutriWave.API.Models.DTO;
using System.Text;
using NutriWave.API.Data;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthenticationController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("CreateAccount")]
    public async Task<IActionResult> CreateAccount([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("User with this email already exists.");
        }

        var user = new UserInformation
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            BirthDate = request.BirthDate,
            PasswordHash = HashPassword(request.Password),
            MedicalConditions = null,
            MedicalReportUrl = null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Account created successfully." });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LogIn([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Later, you can return a JWT here!
        return Ok(new { message = "Login successful." });
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var hashOfInput = HashPassword(password);
        return storedHash == hashOfInput;
    }
}
