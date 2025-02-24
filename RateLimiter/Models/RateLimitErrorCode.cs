using System.Text.Json.Serialization;

namespace RateLimiter.Models
{
    public enum RateLimitErrorCode
    {
        [JsonPropertyName("MISSING_CLIENT_ID")]
        MissingClientId,

        [JsonPropertyName("RATE_LIMIT_EXCEEDED")]
        RateLimitExceeded
    }
}
