
using System;

namespace RateLimiter.Domain.Models
{
    public class RateLimiterStats
    {
        public Guid Id { get; set; }
        public int NumberOfRequestsInTimespan { get; set; }
        public DateTime LastRequestDateTime { get; set; }
    }
}
