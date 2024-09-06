using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace RateLimiter.Config
{

    public class Rule
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("match")]
        public Match? Match { get; set; }

        [JsonPropertyName("conditions")]
        public List<Condition>? Conditions { get; set; }

        [JsonPropertyName("limits")]
        public List<Limit>? Limits { get; set; }
    }
    
}
