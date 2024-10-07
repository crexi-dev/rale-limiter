using Microsoft.AspNetCore.Mvc;
using RateLimiter.Attributes;

namespace RateLimiter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[FixedWindowRateLimit(5, 60)] // 5 requests per 60 seconds
public class ResourceAController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Access to Resource A");
    }
}