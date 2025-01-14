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

        [HttpGet("regional-listings")]
        [RegionRateLimit(usLimit: 5, usWindowSeconds: 60, euCooldownSeconds: 10)]
        public IEnumerable<CommercialListing> GetRegionalListings()
        {
            return new List<CommercialListing>()
            {
                new CommercialListing
                {
                    ListingId = 201,
                    Title = "Pasadena Commercial Space",
                    Price = 2300000,
                    City = "Pasadena",
                    State = "CA",
                    BrokerName = "Bingo Realty"
                },
                new CommercialListing
                {
                    ListingId = 202,
                    Title = "Bronx Office Suite",
                    Price = 1500000,
                    City = "New York",
                    State = "NY",
                    BrokerName = "Fred Realty Corp"
                },
                new CommercialListing
                {
                    ListingId = 203,
                    Title = "Cloud Space",
                    Price = 5500000,
                    City = "New York",
                    State = "NY",
                    BrokerName = "Fred Realty Corp"
                }
            };
        }
    }
}
