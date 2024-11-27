using RateLimiter.Components.RulesService.Models;
using RateLimiter.Models;
using RateLimiter.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiter.Components.RuleService
{
    public class RateLimitingService : IRateLimitingService
    {
        private readonly IEnumerable<IRateLimitingRule> _rules;
        private readonly IEnumerable<RateLimitingRuleGroup> _ruleGroups;

        public RateLimitingService(
            IEnumerable<IRateLimitingRule> rules,
            IEnumerable<RateLimitingRuleGroup> ruleGroups)
        {
            _rules = rules;
            _ruleGroups = ruleGroups;
        }

        public virtual async Task<bool> CanProcessRequestAsync(RateLimitingRequestData requestData, List<string> groups)
        {
            groups ??= new List<string>();

            // add global group so it can be processed
            groups.Add(RateLimitingConstants.GlobalGroupName);

            var selectedRules = SelectRulesToProcess(groups);

            RateLimitingRuleConfiguration ruleConfig = new RateLimitingRuleConfiguration();

            List<Task<bool>> ruleTasks = new();

            foreach (var item in selectedRules) 
            {
                var taskResult = Task.Run(async () =>
                {
                    try
                    {
                        return await item.Rule.RunAsync(requestData, item.Configuration);
                    }
                    catch (Exception ex)
                    {
                        // better logging is needed 
                        Console.WriteLine($"Error processing rule [{item.Rule.GetType().Name}]. Exception: {ex.ToString()}");

                        // returning true to avoid blocking the requests
                        return true;
                    }
                });
                    
                ruleTasks.Add(taskResult);
            }

            var results = await Task.WhenAll(ruleTasks);

            // if there is any problem in the results we let it pass for this sample
            var result = results?.All(item => item) ?? true;  

            return result;
        }

        public virtual List<SelectRulesResult> SelectRulesToProcess(List<string> groups)
        {
            List<SelectRulesResult> result = new();

            var selectedConfigSets = _ruleGroups
                .Where(item => groups.Contains(item.Name))
                .SelectMany(group => group.ConfigurationSets, (group, configSet) => configSet);

            foreach (var configSet in selectedConfigSets)
            {
                var rule = _rules.FirstOrDefault(item => string.Equals(item.GetType().Name, configSet.RuleName, StringComparison.InvariantCultureIgnoreCase));

                if (rule is null)
                {
                    continue;
                }

                result.Add(new SelectRulesResult()
                {
                    Rule = rule,
                    Configuration = configSet.Configuration
                });
            }

            return result;
        }
    }
}
