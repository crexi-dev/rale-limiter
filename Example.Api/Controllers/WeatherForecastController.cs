using Microsoft.AspNetCore.Mvc;
using RateLimiter.Attributes;

namespace Example.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        [CooldownPeriod(2000)]
        [RequestsPerTimespan(1, 1, "Allowed request exceeded")]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("well done");
        }
    }
}
