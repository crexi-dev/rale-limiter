using RateLimitingLibrary.Core.Models;
using System;

namespace RateLimitingLibrary.Rules
{
    /// <summary>
    /// Implements a custom rate limiting rule based on region.
    /// </summary>
    public class CustomRegionRule : BaseRateLimitRule
    {
        public override RateLimitResult Evaluate(ClientRequest request)
        {
            if (request.ClientToken.StartsWith("US") && DateTime.UtcNow.Second % 2 == 0)
            {
                return new RateLimitResult { IsAllowed = false, Message = "Custom region rule violation." };
            }

            return new RateLimitResult { IsAllowed = true };
        }
    }
}