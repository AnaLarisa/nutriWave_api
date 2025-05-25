using NutriWave.API.Models;
using NutriWave.API.Models.DTO;

namespace NutriWave.API.Services.Interfaces;

public interface IAuthService
{
    Task<bool> UserExistsAsync(string email);
    Task CreateUserAsync(RegisterRequest request);
    Task<UserInformation?> AuthenticateUserAsync(string email, string password);
    string GenerateJwtToken(UserInformation user);
}

