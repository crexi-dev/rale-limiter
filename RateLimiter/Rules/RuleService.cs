using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Stores;

namespace RateLimiter.Rules
{
    public class RuleService : IRuleService
    {
        private readonly Dictionary<string, List<IRule>> resourcexRules;
        
        public RuleService()
        {
            this.resourcexRules = new Dictionary<string, List<IRule>>();
        }

        public void AddResourceRules(IEnumerable<IRule> rules, IDictionary<string, IEnumerable<string>> resourcexRuleName)
        {
            if (rules == null) 
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (resourcexRuleName == null)
            {
                throw new ArgumentNullException(nameof(resourcexRuleName));
            }

            if (!resourcexRuleName.Any())
            {
                return;
            }

            Dictionary<string, IRule> ruleNamexRule = new();
            foreach (var rule in rules)
            {
                if (!ruleNamexRule.ContainsKey(rule.Name))
                {
                    ruleNamexRule.Add(rule.Name, rule);
                }
                else
                {
                    throw new Exception($"Multiple rules with the same name: {rule.Name}");
                }
            }

            foreach (var rxn in resourcexRuleName)
            {
                if (!this.resourcexRules.ContainsKey(rxn.Key))
                {
                    this.resourcexRules.Add(rxn.Key, []);
                }

                foreach (var name in rxn.Value)
                {
                    if (ruleNamexRule.ContainsKey(name))
                    {
                        this.resourcexRules[rxn.Key].Add(ruleNamexRule[name]);
                    }
                    else
                    {
                        throw new Exception($"Rule: {name} for resource: {rxn.Key} is unknown. Make sure to register it.");
                    }
                }
            }


        }

        public async Task<bool> Allow(string resource, Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            if (!this.resourcexRules.ContainsKey(resource))
            {
                return true;
            }

            var rules = this.resourcexRules[resource];

            List<Task<bool>> tasks = new();

            foreach (var rule in rules)
            {
                tasks.Add(rule.Allow(client));
            }

            await Task.WhenAll(tasks);

            return tasks.All(t => t.Result);
        }
    }
}
