using System.Text.Json.Serialization;

namespace RateLimiter.Config
{
    public class Match
    {
        [JsonPropertyName("apiUrl")]
        public string ApiUrl { get; set; }
    }
}
