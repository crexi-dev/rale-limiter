using Microsoft.AspNetCore.Mvc;

namespace Crexi.RateLimiterSampleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RateLimiterSampleController : ControllerBase
{
    private readonly ILogger<RateLimiterSampleController> _logger;

    public RateLimiterSampleController(ILogger<RateLimiterSampleController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "RateLimitTest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public string Get(
        [FromHeader(Name="ClientId")] string clientId,
        [FromHeader(Name="RegionCountryCode")] string regionCountryCode, 
        [FromHeader(Name="subscriptionLevel")] string subscriptionLevel)
    {
        _logger.LogInformation($"GET called by {clientId} from {regionCountryCode} using {subscriptionLevel} subscription");
        return $"ClientID {clientId} call allowed at UTC {DateTime.UtcNow:O}";
    }
}