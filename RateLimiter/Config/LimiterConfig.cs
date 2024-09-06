using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RateLimiter.Config
{
    public class LimiterConfig
    {
        [JsonPropertyName("rules")]
        public List<Rule> Rules { get; set; }
    }

}
