namespace RateLimitingLibrary.Core.Models
{
    /// <summary>
    /// Represents the result of a rate limit evaluation.
    /// </summary>
    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }
        public string Message { get; set; } = "Request Allowed";
    }
}