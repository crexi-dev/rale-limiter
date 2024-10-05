namespace RateLimiter
{
    /// <summary>
    /// This class represents whether a request is allowed or denied.
    /// </summary>
    public class RateLimiterResult
    {
        public bool IsAllowed { get; set; }
        public string? Message { get; set; }

        public static RateLimiterResult Allowed() => new RateLimiterResult { IsAllowed = true };
        public static RateLimiterResult Denied(string message) => new RateLimiterResult { IsAllowed = false, Message = message };
    }
}
