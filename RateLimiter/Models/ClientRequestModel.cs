using System;

namespace RateLimiter.Models
{
    public record ClientRequestModel(string URI, DateTime Timestamp);
}
