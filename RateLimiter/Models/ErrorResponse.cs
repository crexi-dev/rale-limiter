using System.Text.Json.Serialization;

namespace RateLimiter.Models
{
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public ErrorDetails Error { get; set; }

  
    }
}
