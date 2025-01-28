using System;
using System.Collections.Generic;
using System.Linq;
using RateLimiter.Core.Configuration.Contracts;

namespace RateLimiter.Core.Configuration;
public class RateLimiter(ILogger logger = null!)
{
    private readonly Dictionary<string, List<IRateLimitRule>> _resourceRules = new();

    public void AddRule(string resourceKey, IRateLimitRule rule)
    {
        if (string.IsNullOrEmpty(resourceKey))
            throw new ArgumentNullException(nameof(resourceKey));
        
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        try
        {
            if (!_resourceRules.ContainsKey(resourceKey))
                _resourceRules[resourceKey] = new List<IRateLimitRule>();
            
            _resourceRules[resourceKey].Add(rule);
            logger.LogInformation($"Added rule {rule.GetType().Name} for resource {resourceKey}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error adding rule for resource {resourceKey}", ex);
            throw;
        }
    }
    

    public bool IsRequestAllowed(string clientToken, string resourceKey)
    {
        if (string.IsNullOrEmpty(clientToken))
            throw new ArgumentNullException(nameof(clientToken));
        
        if (string.IsNullOrEmpty(resourceKey))
            throw new ArgumentNullException(nameof(resourceKey));

        try
        {
            if (!_resourceRules.TryGetValue(resourceKey, out var rules) || !rules.Any())
            {
                logger.LogWarning($"No rules configured for resource {resourceKey}");
                return true;
            }

            foreach (var rule in rules.Where(rule => !rule.IsAllowed(clientToken, resourceKey)))
            {
                logger.LogWarning($"Request blocked by {rule.GetType().Name} " +
                                  $"for {clientToken} on {resourceKey}");
                return false;
            }

            foreach (var rule in rules)
            {
                try
                {
                    rule.RecordRequest(clientToken, resourceKey);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error recording request in {rule.GetType().Name}", ex);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error processing request for {clientToken} on {resourceKey}", ex);
            return false;
        }
    }
}