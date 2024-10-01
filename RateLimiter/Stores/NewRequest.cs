using System;

namespace RateLimiter.Stores
{
    public record NewRequest(string ClientId, DateTimeOffset Date)
    {
    }
}
