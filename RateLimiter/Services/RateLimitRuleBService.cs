using RateLimiter.Interfaces;
using System;

namespace RateLimiter.Services
{
    public class RateLimitRuleBService : IRateLimitRule
    {
        public bool IsRequestAllowed(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
