using Microsoft.AspNetCore.Mvc;

namespace RateLimiter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimitTestController : ControllerBase
    {
        [HttpGet("resource1")]
        public IActionResult GetResource1()
        {
            return Ok(new { message = "Access granted to resource1!" });
        }

        [HttpGet("resource2")]
        public IActionResult GetResource2()
        {
            return Ok(new { message = "Access granted to resource2!" });
        }
    }
}
