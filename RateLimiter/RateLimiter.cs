using System;
using System.Collections.Generic;
using System.Linq;
using RateLimiterNS.RateLimitRules;
using System.Xml.Linq;

namespace RateLimiterNS.RateLimiter
{
    public class RateLimiter
    {
        private readonly Dictionary<string, List<IRateLimitRule>> _tokenRules;

        public RateLimiter(Dictionary<string, List<IRateLimitRule>> tokenRules)
        {
            _tokenRules = tokenRules;
        }

        public bool IsRequestAllowed(string token)
        {
            if (!_tokenRules.ContainsKey(token))
            {
                Console.WriteLine($"Invalid token: {token}");
                return false;
            }

            var requestTime = DateTime.UtcNow;
            return _tokenRules[token].All(rule => rule.IsRequestAllowed(token, requestTime));
        }

        public static RateLimiter LoadFromConfiguration(string xmlFilePath)
        {
            var doc = XDocument.Load(xmlFilePath);
            var tokenRules = new Dictionary<string, List<IRateLimitRule>>();

            foreach (var tokenElement in doc.Descendants("Token"))
            {
                // Ensure token is non-null
                string? token = tokenElement.Attribute("Value")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    continue; // Skip this token if it's null or empty
                }

                var rules = new List<IRateLimitRule>();

                foreach (var ruleElement in tokenElement.Descendants("Rule"))
                {
                    string? ruleType = ruleElement.Attribute("Type")?.Value;
                    if (ruleType == "RequestsPerTimeSpan")
                    {
                        if (int.TryParse(ruleElement.Element("MaxRequests")?.Value, out int maxRequests) &&
                            int.TryParse(ruleElement.Element("TimeSpanMinutes")?.Value, out int timeSpanMinutes))
                        {
                            TimeSpan timespan = TimeSpan.FromMinutes(timeSpanMinutes);
                            rules.Add(new RequestsPerTimeSpanRule(maxRequests, timespan));
                        }
                    }
                    else if (ruleType == "TimeSpanSinceLastRequest")
                    {
                        if (int.TryParse(ruleElement.Element("MinTimeSpanSeconds")?.Value, out int minTimeSpanSeconds))
                        {
                            TimeSpan minTimeSpan = TimeSpan.FromSeconds(minTimeSpanSeconds);
                            rules.Add(new TimeSpanSinceLastRequestRule(minTimeSpan));
                        }
                    }
                }

                tokenRules[token] = rules;
            }

            return new RateLimiter(tokenRules);
        }
    }
}

