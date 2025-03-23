using Microsoft.AspNetCore.Mvc;

namespace NutriWave.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{

    [HttpGet(Name = "GetWeatherForecast")]
    public IList<string> Get()
    {
        return new List<string> { "value1", "value2" };
    }
}
