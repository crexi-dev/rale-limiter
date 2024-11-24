using Crexi.API.Common.RateLimiter.Interfaces;
using Crexi.API.Common.RateLimiter.Rules;

namespace Crexi.Auctions.API.Middleware.RateLimiter
{
    public static class RateLimiterConfigurator
    {
        public static void ConfigureRateLimitingRules(IRateLimiter rateLimiter)
        {
            var usRateLimitRule = new FixedWindowRateLimitRule(
                maxRequestsPerClient: 10,
                timeWindow: TimeSpan.FromSeconds(10)
            );

            var euRateLimitRule = new TimeSinceLastCallRule(
                requiredInterval: TimeSpan.FromSeconds(2)
            );

            var usConditionalRule = new ConditionalRateLimitRule(
                condition: client => client.Location == "US",
                rule: usRateLimitRule
            );

            var euConditionalRule = new ConditionalRateLimitRule(
                condition: client => client.Location == "EU",
                rule: euRateLimitRule
            );

            var compositeRule = new CompositeRateLimitRule(new[]
            {
                usConditionalRule,
                euConditionalRule
            });

            rateLimiter.ConfigureResource("/Auctions", compositeRule);

            var anotherRule = new FixedWindowRateLimitRule(50, TimeSpan.FromMinutes(10));
            rateLimiter.ConfigureResource("/AuctionSettings", anotherRule);
        }
    }
}
