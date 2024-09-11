using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RateLimiter;

public class RateLimiter(
    IMemoryCache cache,
    IRateLimitRuleRepository ruleRepository,
    IOptions<RateLimiterConfig> rateLimiterConfig,
    ILogger<RateLimiter> logger) : IRateLimiter
{
    private readonly IMemoryCache _cache = Guard.Against.Null(cache);
    private readonly IRateLimitRuleRepository _ruleRepository = Guard.Against.Null(ruleRepository);
    private readonly RateLimiterConfig _rateLimiterConfig = Guard.Against.Null(rateLimiterConfig.Value);
    private readonly ILogger<RateLimiter> _logger = Guard.Against.Null(logger);

    private readonly ConcurrentDictionary<string, HashSet<IRateLimitRule>> _resourceRules = new();

    public async Task<bool> IsRequestAllowed(string clientId, string resource)
    {
        Guard.Against.NullOrEmpty(clientId);
        Guard.Against.NullOrEmpty(resource);

        var cachedRules = await GetCachedRules(clientId);
        if (!cachedRules.Any())
        {
            cachedRules = await _ruleRepository.GetRulesByClientId(clientId);

            if (!cachedRules.Any())
            {
                //TODO No rate limiting rules found for the client; we could either allow all requests or apply a default rule.
                _logger.LogWarning($"No rate limiting rules found for client: {clientId}");
                return false;
            }

            await ConfigureRules(clientId, cachedRules);
        }

        var key = BuildClientResourceKey(clientId, resource);
        if (!_resourceRules.TryGetValue(key, out var rules))
        {
            //TODO No rate limiting rules found for the client; we could either allow all requests or apply a default rule.
            return false;
        }

        var tasks = rules.Select(async rule => await rule.IsRequestAllowed(clientId, resource));
        var results = await Task.WhenAll(tasks);
        return results.All(result => result);
    }

    #region Private Method

    /// <summary>
    /// Configures rules from the database
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="rules"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private Task ConfigureRules(string clientId, IReadOnlyCollection<RateLimitRuleEntity> rules)
    {
        var cacheKey = BuildRateLimitCacheKey(clientId);
        CacheRules(cacheKey, rules);

        foreach (var ruleEntity in rules)
        {
            IRateLimitRule rule = ruleEntity.RuleType switch
            {
                RuleType.RequestCount => new RequestCountRule(ruleEntity.MaxRequests, ruleEntity.TimeSpan),
                RuleType.TimeSpan => new TimeSpanRule(ruleEntity.RequiredTimeSpan),
                _ => throw new ArgumentException("Unsupported rule type")
            };

            var key = BuildClientResourceKey(ruleEntity.ClientId, ruleEntity.Resource);
            _resourceRules.AddOrUpdate(key, [rule], (_, existingRules) =>
            {
                existingRules.Add(rule);
                return existingRules;
            });
        }

        return Task.CompletedTask;
    }

    private Task<IReadOnlyCollection<RateLimitRuleEntity>> GetCachedRules(string clientId)
    {
        Guard.Against.NullOrWhiteSpace(clientId);

        _cache.TryGetValue(BuildRateLimitCacheKey(clientId), out List<RateLimitRuleEntity>? cachedRules);

        return Task.FromResult((IReadOnlyCollection<RateLimitRuleEntity>)(cachedRules ?? []));
    }

    private string BuildRateLimitCacheKey(string clientId)
    {
        return $"RateLimitRules_{clientId}";
    }

    private string BuildClientResourceKey(string clientId, string resource)
    {
        return $"{clientId}-{resource}";
    }

    private void CacheRules(string cacheKey, IReadOnlyCollection<RateLimitRuleEntity> rules)
    {
        _cache.Set(cacheKey, rules, TimeSpan.FromMinutes(_rateLimiterConfig.CacheExpirationInMinutes));
    }

    #endregion
}