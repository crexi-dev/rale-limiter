namespace RateLimiter.Services
{
    public class NorthAmericanRateLimiterPolicy(ClientBehaviorCache clientBehaviorCache, IConfiguration configuration) : IRateLimiterPolicy
    {
        public bool IsApplicable(string apiKey, DateTime requestTime) => IsRateLimitedBySlidingWindow(apiKey, requestTime);

        private bool IsRateLimitedBySlidingWindow(string apiKey, DateTime requestTime)
        {
            int maxRequests = Convert.ToInt32(configuration["RateLimiting:SlidingWindow:MaxRequests"]);
            int slidingWindowLengthSeconds = Convert.ToInt32(configuration["RateLimiting:SlidingWindow:WindowLengthSeconds"]);

            var requestsInWindow = clientBehaviorCache
                .Get(apiKey)
                .Where(r => r.AddSeconds(slidingWindowLengthSeconds) >= requestTime);

            return requestsInWindow.Count() > maxRequests;
        }
    }
}
