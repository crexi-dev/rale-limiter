using CrexiService.Models;
using Microsoft.AspNetCore.Mvc;
using RateLimiter.Attributes;

namespace CrexiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        [HttpGet("listings")]
        [RateLimit(1, 5)]
        public IEnumerable<CommercialListing> GetListings()
        {
            return new List<CommercialListing>()
            {
                new CommercialListing
                {
                    ListingId = 101,
                    Title = "Downtown Retail Space",
                    Price = 2500000,
                    City = "Los Angeles",
                    State = "CA",
                    BrokerName = "Alice Realty"
                },
                new CommercialListing
                {
                    ListingId = 102,
                    Title = "Midtown Office Suite",
                    Price = 1500000,
                    City = "New York",
                    State = "NY",
                    BrokerName = "Spongebob & Associates"
                }
            };
        }
    }
}
