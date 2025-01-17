using System.Collections.Generic;

namespace RateLimitingLibrary.Config
{
    /// <summary>
    /// Configuration for defining rules associated with specific API resources.
    /// </summary>
    public class RuleConfigurations
    {
        public Dictionary<string, List<string>> ResourceRules { get; set; } = new();
    }
}