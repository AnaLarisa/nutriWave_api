namespace NutriWave.API.Models.DTO;

public class InfoRequest
{
    public required string Description { get; set; }

    public required int UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}
