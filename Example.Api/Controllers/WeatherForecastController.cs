using Example.Api.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Example.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        [CooldownPeriod(2000)]
        [RequestsPerTimespan(1, 2000, "Allowed request exceeded")]
        [EuOriginUser(1000)]
        [UsOriginUser(1000, 10)]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("well done");
        }
    }
}
