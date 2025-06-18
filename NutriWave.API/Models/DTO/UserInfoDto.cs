namespace NutriWave.API.Models.DTO;

public class UserInfoDto
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public DateTime BirthDate { get; set; }

    public Sex Sex { get; set; }

    public string[]? MedicalConditions { get; set; }
}
