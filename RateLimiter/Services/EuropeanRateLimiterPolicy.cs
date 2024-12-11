namespace RateLimiter.Services
{
    public class EuropeanRateLimiterPolicy(ClientBehaviorCache clientBehaviorCache, IConfiguration configuration) : IRateLimiterPolicy
    {
        public bool IsApplicable(string apiKey, DateTime requestTime) => IsRateLimitedByFixedWindow(apiKey, requestTime);

        private bool IsRateLimitedByFixedWindow(string apiKey, DateTime requestTime)
        {
            int maxRequests = Convert.ToInt32(configuration["RateLimiting:FixedWindow:MaxRequests"]);
            string fixedWindowUnit = configuration["RateLimiting:FixedWindow:UnitOfTime"]!;

            Func<DateTime, bool> fixedWindowPredicate = fixedWindowUnit switch
            {
                "Second" => r => r.Minute == requestTime.Second,
                "Minute" => r => r.Minute == requestTime.Minute,
                "Hour" => r => r.Minute == requestTime.Hour,
                _ => throw new ArgumentException($"Invalid unit: {fixedWindowUnit}")
            };

            var requestsInWindow = clientBehaviorCache
                .Get(apiKey)
                .Where(fixedWindowPredicate);

            return requestsInWindow.Count() > maxRequests;
        }
    }
}
