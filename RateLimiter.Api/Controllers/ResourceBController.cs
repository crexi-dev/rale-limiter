using Microsoft.AspNetCore.Mvc;
using RateLimiter.Attributes;

namespace RateLimiter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceBController : ControllerBase
{
    [HttpGet]
    [FixedDelayRateLimit(10)] // 10 seconds delay between requests
    public IActionResult Get()
    {
        return Ok("Access to Resource B");
    }
}