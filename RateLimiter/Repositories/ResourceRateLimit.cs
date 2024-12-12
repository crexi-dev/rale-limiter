using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Repositories
{
    public class ResourceRateLimit : IResourceRateLimit
    {
        private readonly Dictionary<string, List<IRateLimitingRule>> _rulesCollection = new Dictionary<string, List<IRateLimitingRule>>();

        /// <summary>
        /// Add this combination of resource (Resource.Region) and rule to the collection
        /// </summary>
        /// <param name="resource">Resource is formatted as Resource.Region</param>
        /// <param name="rule">Add rule to this resource</param>
        public void AddRuleResource(string resource, IRateLimitingRule rule)
        {
            if(!_rulesCollection.ContainsKey(resource))
                _rulesCollection[resource] = new List<IRateLimitingRule>();

            _rulesCollection[resource].Add(rule);
        }

        /// <summary>
        /// Check if this client has broken a rule in the collection
        /// </summary>
        /// <param name="resource">Resource is in the format of Resource.Region</param>
        /// <param name="client">The client with a specific region</param>
        /// <returns>Returns ResultResult.IsSuccess is False if a rule is broken along with the broken rule message else ResultResult.IsSuccess is True.</returns>
        public RuleResult CheckRule(string resource, IClient client)
        {
            var resourceRegion = resource + "." + client.ReturnRegion().ToString();

            if (!_rulesCollection.ContainsKey(resourceRegion))
                return null;

            RuleResult ruleResult = null;

            ParallelOptions parallelOptions = new ParallelOptions
            {
                //Use the number of available CPU threads. Leaving one thread for other tasks
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            Parallel.ForEach(_rulesCollection[resourceRegion], parallelOptions, (rule, state) =>
            {
                ruleResult = rule.Check(client.ReturnLoggedTimes());

                if (!ruleResult.IsSuccess)
                    state.Stop();
            });

            return ruleResult;
        }

        /// <summary>
        /// Returns the collection of configured rules for resources and regions.
        /// </summary>
        /// <returns>Returns a dictionary<string, List<IRateLimitingRule> Object</returns>
        public Dictionary<string, List<IRateLimitingRule>> GetCollection()
        {
            return _rulesCollection;
        }
    }
}
