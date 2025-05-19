namespace NutriWave.API.Models.DTO;

public class GetInfoRequest
{
    public required string Description { get; set; }

    public required int UserId { get; set; }
}
