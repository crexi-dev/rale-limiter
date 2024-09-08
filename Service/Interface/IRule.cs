using System;

namespace RateLimiter.Service.Interface
{
    public interface IRule
    {
        bool Allow(string resource, string token, DateTime requestTime);
    }
}
