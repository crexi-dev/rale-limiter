using Microsoft.AspNetCore.Mvc;

namespace Crexi.Auctions.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuctionsController : ControllerBase
    {
        private readonly ILogger<AuctionsController> _logger;

        public AuctionsController(ILogger<AuctionsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public int Get()
        {
            return 1;
        }
    }
}
