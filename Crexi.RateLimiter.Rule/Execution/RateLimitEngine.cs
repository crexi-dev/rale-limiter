using Crexi.RateLimiter.Rule.Constants;
using Crexi.RateLimiter.Rule.Extensions;
using Crexi.RateLimiter.Rule.Model;
using Crexi.RateLimiter.Rule.ResourceAccess;
using Crexi.RateLimiter.Rule.Utility;

namespace Crexi.RateLimiter.Rule.Execution;

public class RateLimitEngine(IRateLimitResourceAccess resourceAccess, TimeProvider timeProvider, IRuleEvaluationLogic logic): IRateLimitEngine
{
    public void AddUpdateRules(IEnumerable<RateLimitRule> rules)
    {
        var comparer = new UpdateRateLimitRuleComparer();
        var ruleSets = rules.GroupBy(r => r.ToCallData(), new CallDataComparer());
        foreach (var ruleSet in ruleSets)
        {
            var existingRuleDictionary = resourceAccess.GetRules(ruleSet.Key)?
                .ToDictionary(comparer.GetHashCode) ?? new Dictionary<int, RateLimitRule>();
            foreach (var rule in ruleSet)
            {
                var hash = comparer.GetHashCode(rule);
                existingRuleDictionary[hash] = rule;
            }
            resourceAccess.SetRules(existingRuleDictionary.Values, ruleSet.Key);
            resourceAccess.SetExpirationWindow(TimeSpan.FromMilliseconds(existingRuleDictionary.Values.Max(r => r.Timespan)), ruleSet.Key);
        }
    }

    public (bool success, int? responseCode) Evaluate(CallData callData)
    {
        var rules = GetMostSpecificRulesForCallData(callData);
        if (rules is null || rules.Count == 0) return ResultConstants.SuccessResponse;
        var callHistory = resourceAccess.AddCallAndGetHistory(callData);
        return EvaluateRules(rules, callHistory);
    }

    #region private methods

    private IList<RateLimitRule>? GetMostSpecificRulesForCallData(CallData callData)
    {
        IList<RateLimitRule>? rules;
        do
        {
            rules = resourceAccess.GetRules(callData);
        } while (rules is null && TryDeSpecifyCallDataByOne(callData));
        return rules;
    }

    private static bool TryDeSpecifyCallDataByOne(CallData callData)
    {
        if (callData.RegionId.HasValue)
        {
            callData.RegionId = null;
            return true;
        }
        if (callData.TierId.HasValue)
        {
            callData.TierId = null;
            return true;
        }
        if (callData.ClientId.HasValue)
        {
            callData.ClientId = null;
            return true;
        }
        return false;
    }

    private (bool success, int? responseCode) EvaluateRules(IList<RateLimitRule> rules, CallHistory callHistory)
    {
        foreach (var rule in rules)
        {
             if (!IsInEffectiveWindow(rule)) continue;
             var result = logic.EvaluateRule(rule.EvaluationType, TimeSpan.FromMilliseconds(rule.Timespan), rule.MaxCallCount, rule.OverrideResponseCode, callHistory);
             if (!result.success)
                 return (false, rule.OverrideResponseCode ?? ResultConstants.DefaultFailureResponseCode);
        }
        return ResultConstants.SuccessResponse;
    }

    private bool IsInEffectiveWindow(RateLimitRule rule)
    {
        if (rule.EffectiveWindowStartUtc is null)
            return true;
        var currentUtc = TimeOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        return rule.EffectiveWindowStartUtc <= currentUtc && rule.EffectiveWindowEndUtc >= currentUtc;
    }

    #endregion private methods
}