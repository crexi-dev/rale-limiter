using System.ComponentModel;
using System.Text.Json.Serialization;
using Services.Common.Utilities;

namespace Services.Common.Configurations;

public class RuleConfig
{
    // Unique identifier for the resource this rule applies to, e.g., an API endpoint path
    [JsonPropertyName("resourceIdentifier")]
    public string ResourceIdentifier { get; set; }
    
    // Type of the rule (e.g., "RequestPerTimespan", "TimeSinceLastCall")
    [JsonPropertyName("ruleTypes")]
    public List<string> RuleTypes { get; set; }
    
    // Limit on the number of requests (used for "RequestPerTimespan" rules)
    [JsonPropertyName("requestLimit")]
    public int? RequestLimit { get; set; }

    // Timespan for request limit (used for "RequestPerTimespan" rules)
    [JsonPropertyName("timespan")]
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    public TimeSpan? Timespan { get; set; }

    // Minimum interval required between requests (used for "TimeSinceLastCall" rules)
    [JsonPropertyName("minIntervalBetweenRequests")]
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    public TimeSpan? MinIntervalBetweenRequests { get; set; }
    
    // Region-based configuration, such as "US" or "EU", which allows different rules by region
    [JsonPropertyName("region")]
    public string Region { get; set; }

    // Additional parameters or metadata can be added here as needed
}