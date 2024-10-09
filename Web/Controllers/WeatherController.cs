using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController() : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Get(string token)
    {
        return Task.FromResult<IActionResult>(Ok(new { Weather = "Sunny", Temperature = 28 }));
    }
}
