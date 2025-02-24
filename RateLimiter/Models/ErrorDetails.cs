using System.Text.Json.Serialization;

namespace RateLimiter.Models
{
    public class ErrorDetails
    {
        [JsonPropertyName("code")]
        public RateLimitErrorCode Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("retry_after")]
        public double? RetryAfter { get; set; }
    }
}
