namespace NutriWave.API.Models.DTO;

public class FoodIntakeRequest
{
    public required string FoodDescription { get; set; }

    public required int UserId { get; set; }
}
