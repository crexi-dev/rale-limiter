using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System;

public class RateLimiterManager
{
  private readonly ConcurrentDictionary<string, List<IRateLimitRule>> _resourceRules;

  public RateLimiterManager()
  {
    _resourceRules = new ConcurrentDictionary<string, List<IRateLimitRule>>();
  }

  public void AddRule(string resource, IRateLimitRule rule)
  {
    _resourceRules.AddOrUpdate(resource,
        new List<IRateLimitRule> { rule },
        (key, existingRules) =>
        {
          existingRules.Add(rule);
          return existingRules;
        });
  }

  public bool IsRequestAllowed(string clientId, string resource, string region, string token)
  {
    bool isAllowed = false; // This will store whether at least one rule allows the request

    // Iterate through all rules
    foreach (var rule in _resourceRules[resource])
    {
      bool ruleResult = rule.IsRequestAllowed(clientId, resource, region, token);

      // We want to update all rules' counts regardless of whether one already allowed the request
      if (ruleResult)
      {
        isAllowed = true; // If any rule allows the request, we set isAllowed to true
      }
    }

    // Return true if any rule allowed the request, false otherwise
    return isAllowed;
  }
}
