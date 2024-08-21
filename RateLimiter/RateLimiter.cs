using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Rules;

namespace RateLimiter
{
    public class RateLimiter
    {
        /// <summary>
        /// Contains API Resource as Key and list of rate-limiting rules as Value.
        /// </summary>
        private Dictionary<string, List<Rule>> resources = new Dictionary<string, List<Rule>>();

        public RateLimiter()
        {
        }

        /// <summary>
        /// Set rules for a resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="rules"></param>
        /// <exception cref="Exception"></exception>
        public void SetRules(string resource, List<Rule> rules)
        {
            resources[resource] = rules;
        }

        /// <summary>
        /// Mimics sending multiple requests through the rate limiter.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="count"></param>
        /// <returns>Boolean - whether or not any request is blocked.</returns>
        public bool SendRequests(string resource, int count, string accessToken)
        {
            for (int i = 0; i < count; i++){
                
                var success = this.SendRequest(resource, accessToken);
                if (!success) return false;
            }

            return true;
        }

        /// <summary>
        /// Mimics sending a request through the rate limiter.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns>Boolean - whether or not the request is blocked.</returns>
        public bool SendRequest(string resource, string accessToken)
        {
            List<Rule> rules = resources.GetValueOrDefault(resource, null);

            if (rules == null) return true;

            foreach (var rule in rules)
            {
                if (!rule.SendRequest(accessToken)) return false;
            }

            return true;
        }

    }
}
