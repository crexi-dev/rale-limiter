using Microsoft.AspNetCore.Mvc;
using RateLimiter.Attributes;

namespace RateLimiter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[GeoBasedRateLimit("US",10, 60)] // US: 10 req/60 sec
[GeoBasedRateLimit("EU", 0, 15)] // EU: 15 sec delay
public class ResourceDController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Access to Resource D");
    }
}