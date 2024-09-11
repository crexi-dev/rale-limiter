using Microsoft.AspNetCore.Mvc;
using TestApi.Common;

namespace TestApi.Controllers
{
    [ApiController]
    public class ResourceController : ControllerBase
    {
        [HttpGet(Routes.ResourceA)]
        public IActionResult GetResourceA()
        {
            return Ok("Accessed Resource A");
        }

        [HttpGet(Routes.ResourceB)]
        public IActionResult GetResourceB()
        {
            return Ok("Accessed Resource B");
        }

        [HttpGet(Routes.ResourceC)]
        public IActionResult GetResourceC()
        {
            return Ok("Accessed Resource C");
        }
    }
}
