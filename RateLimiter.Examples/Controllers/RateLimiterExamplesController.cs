using Microsoft.AspNetCore.Mvc;
using RateLimiter.Examples.Rules;

namespace RateLimiter.Examples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RateLimiterExamplesController(IRateLimiterStorage rlStorage) : ControllerBase
    {
        private readonly IRateLimiterStorage rlStorage = rlStorage;

        [HttpGet("defaultFixedWindow")]
        [RateLimiter(RateLimiterPolicyNames.DefaultFixedWindowPolicy)]
        public ActionResult RestrictedWithDefaultFixedWindowPolicy()
        {
            return Ok();
        }

        [HttpGet("customPolicy")]
        [RateLimiter(RateLimiterPolicyNames.MyCustomComplexGeoPolicy)]
        public ActionResult RestrictedWithCustomPolicy()
        {
            return Ok();
        }

        [HttpPost("restrictedInsideAction")]
        public async Task<ActionResult> RestrictedInsideAction(CancellationToken ct = default)
        {
            if (!await RateLimiter.IsRequestAllowedAsync(HttpContext, RateLimiterPolicyNames.MyCustomRequestsSizePolicy, ct))
            {
                return StatusCode(403);
            }
            return Ok();
        }

        [HttpGet("restrictedWithoutPolicy")]
        public async Task<ActionResult> RestrictedWithoutPolicy([FromHeader] string location, CancellationToken ct = default)
        {
            if (await RateLimiter.IsRequestAllowedAsync(HttpContext, [new MyCustomRequestSizeRule(TimeSpan.FromSeconds(10), 20 * 1024, rlStorage, location)], ct))
            {
                return StatusCode(403);
            }
            return Ok();
        }

        [HttpGet("twoPolicies")]
        [RateLimiter(RateLimiterPolicyNames.DefaultSlidingWindowPolicy)]
        [RateLimiter(RateLimiterPolicyNames.DefaultFixedWindowPolicy)]
        public ActionResult RestrictedWithTwoPolicies()
        {
            return Ok();
        }
    }
}
