using Microsoft.AspNetCore.Mvc;
using RateLimiter.Attributes;

namespace Example.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {


        [RequestsPerTimespan(10, 10)]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("well done");
        }
    }
}
