using Microsoft.AspNetCore.Mvc;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using RateLimiter.Models.Apis;

namespace RateLimiterWebApi.Controllers
{
    /// <summary>
    /// Sample class to check policy.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly ILogger<ResourceController> _logger;
        private readonly IRateLimitService _rateLimitService;

        public ResourceController(ILogger<ResourceController> logger, IRateLimitService rateLimitService)
        {
            _logger = logger;
            _rateLimitService = rateLimitService;
        }

        [HttpGet]
        [Route("{resourceId}/user/{userId}")]
        public async Task<RateLimiteResponse> GetResourceAccess(string resourceId, string userId)
        {
            var result = await _rateLimitService.CheckRateLimitAsync(new RateLimitRequest()
            {
                UserId = userId,
                ResourceId = resourceId,
            });

            return result;
        }
    }
}
