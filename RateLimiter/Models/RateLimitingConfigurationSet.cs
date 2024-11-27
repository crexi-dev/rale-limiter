namespace RateLimiter.Models
{
    public class RateLimitingConfigurationSet
    {
        public RateLimitingConfigurationSet(string ruleName) 
        { 
            RuleName = ruleName;
        }
        
        public RateLimitingConfigurationSet()
        {

        }

        public string RuleName { get; set; } = default!;
        public RateLimitingRuleConfiguration Configuration { get; set; } = default!;
    }
}
