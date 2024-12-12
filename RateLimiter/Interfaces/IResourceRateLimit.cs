using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Models;

namespace RateLimiter.Interfaces
{
    public interface IResourceRateLimit
    {
        public void AddRuleResource(string resource, IRateLimitingRule rule);
        public Dictionary<string, List<IRateLimitingRule>> GetCollection();
        public RuleResult CheckRule(string resource, IClient client);
    }
}
