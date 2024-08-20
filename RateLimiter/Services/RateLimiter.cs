using System.Collections.Generic;
using RateLimiter.Contracts;

namespace RateLimiter.Services;

public class RateLimiter(List<IRateLimitRule> limitRules)
{
    public bool IsRequestAllowed(string clientToken)
    {
        foreach (var limitRule in limitRules)
        {
            if (!limitRule.IsRequestAllowed(clientToken))
            {
                return false;
            }
        }

        return true;
    }
}