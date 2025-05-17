using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using NutriWave.API.Data;
using NutriWave.API.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NutriWave.API.Helpers;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services;

public class AuthService(
    AppDbContext context,
    IConfiguration configuration,
    INutrientRequirementService nutrientRequirementService,
    INutrientIntakeService nutrientIntakeService)
    : IAuthService
{
    public string GenerateJwtToken(UserInformation user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["JwtSettings:Secret"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(configuration["JwtSettings:ExpiryMinutes"])),
            Issuer = configuration["JwtSettings:Issuer"],
            Audience = configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task CreateUserAsync(RegisterRequest request)
    {
        var user = new UserInformation
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            BirthDate = request.BirthDate,
            Sex = GetSex(request.Sex),
            PasswordHash = HashPassword(request.Password),
            MedicalConditions = null,
            MedicalReportUrl = null
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var age = CalculationsHelper.CalculateAge(user.BirthDate);
        await nutrientRequirementService.AddUserNutrientRequirements(user.UserId, user.Sex, age);
    }

    private static Sex GetSex(string requestSex)
    {
        return requestSex.ToLowerInvariant() switch
        {
            "feminin" or "female" or "f" => Sex.Female,
            "masculin" or "male" or "m" => Sex.Male,
            _ => throw new ArgumentException("The sex of the user is incorrect.")
        };
    }

    public async Task<UserInformation?> AuthenticateUserAsync(string email, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        await nutrientIntakeService.AddNewNutrientIntakeForTodayIfNotPresent(user.UserId);

        return user;
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