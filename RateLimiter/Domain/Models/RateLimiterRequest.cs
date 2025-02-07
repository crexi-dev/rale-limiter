using System;

namespace RateLimiter.Domain.Models
{
    public class RateLimiterRequest
    {
        public Guid Id { get; set; }
        public Enumerations.Contries Country {get; set;}
        public DateTime RequestDate { get; set;}
    }
}
