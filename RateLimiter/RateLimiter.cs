using System;
using Newtonsoft.Json;
using RateLimiter.Enums;
using System.Reflection;
using System.Linq;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using StackExchange.Redis;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RateLimiter
{
    public class RateLimiter
    {
        private IDatabase _database;
        /// <summary>
        /// Constructor
        /// </summary>
        public RateLimiter(IDatabase database) 
        {
            _database = database;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="ruleType"></param>
        /// <param name="region"></param>
        /// <returns>bool</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool IsRequestRateLimited(string accessToken, RuleType ruleType, string? region)
        {
            bool isRequestRateLimited = false;
            
            //Get relevant Rules based on ruleType and region (if applicable)    
            var config = GetRulesFromJSONFile();

            var userRules = from rule in config.Rules
                            where rule.Active
                            && rule.Type == ruleType
                            && rule.Region == region
                            select rule;

            if (!userRules.Any()) {
                throw new InvalidOperationException("No applicable rules found.");
            }

            // run all rules to see if this request is rate limited.
            foreach (var rule in userRules.Select(CreateRule).ToList()) {
                
                isRequestRateLimited = rule.ShouldRateLimit(accessToken, DateTime.UtcNow);
                
                if (isRequestRateLimited)
                {
                    return true;
                }
            }
            return isRequestRateLimited;
        }

        #region Private methods
        
        /// <summary>
        /// Read the Json files and retrieve all the rules
        /// </summary>
        /// <returns>RateLimiterConfig</returns>
        private RateLimiterConfig GetRulesFromJSONFile()
        {
            string jsonContent = "";

            // Get the executing assembly
            var assembly = Assembly.GetExecutingAssembly();

            // Name of the embedded resource in the format: Namespace.FolderName.FileName
            var resourceName = "RateLimiter.RateLimitRules.json";

            // Open the resource stream
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Resource not found.", resourceName);
                }

                using (var reader = new StreamReader(stream))
                {
                    jsonContent = reader.ReadToEnd();
                }
            }
            // Deserialize the JSON content into a RateLimiterConfig object
            var config = JsonConvert.DeserializeObject<RateLimiterConfig>(jsonContent) ?? new RateLimiterConfig();

            return config;
        }

        /// <summary>
        /// Creates RateLimiterRule based on rule strategy
        /// </summary>
        /// <param name="rule"></param>
        /// <returns>IRateLimiterRule</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private IRateLimiterRule CreateRule(RateLimiterRule rule)
        {
            return rule.RuleStrategy switch
            {
                RuleStrategy.FixedNumOfRequests => new FixedNumOfRequestsRule(rule.MaxRequests.Value, rule.TimespanSec.Value, _database),
                RuleStrategy.MinInterval => new MinIntervalRule(rule.MininumIntervalSec.Value, _database),
                _ => throw new InvalidOperationException($"Unknown rule type: {rule.Name}")
            };
        }

        #endregion

    }
}
