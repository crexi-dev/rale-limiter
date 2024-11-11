using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Services.Common.RateLimitRules;
using Services.Common.Repositories;

namespace Services.Common.Configurations;

public class RuleConfigLoader : BackgroundService, IRuleConfigLoader
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IRateLimitRuleFactory _ruleFactory;
    private readonly ConcurrentDictionary<string, IEnumerable<IRateLimitRule>> _rulesCache;

    public RuleConfigLoader(IRuleRepository ruleRepository, IRateLimitRuleFactory ruleFactory)
    {
        _ruleRepository = ruleRepository;
        _ruleFactory = ruleFactory;
        _rulesCache = new ConcurrentDictionary<string, IEnumerable<IRateLimitRule>>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var ruleConfigs = await _ruleRepository.GetRuleConfigsAsync();
            UpdateRulesCache(ruleConfigs);
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Adjust refresh interval
        }
    }

    private void UpdateRulesCache(IList<RuleConfig> ruleConfigs)
    {
        foreach (var config in ruleConfigs)
        {
            _rulesCache[$"{config.Region}:{config.ResourceIdentifier}".ToLower()] = CreateRulesFromConfig(config); // Convert RuleConfig to IRateLimitRule
        }
    }

    public IEnumerable<IRateLimitRule> GetRulesForResource(string resource, string region) =>
        _rulesCache.TryGetValue($"{region}:{resource}".ToLower(), out var rules) ? rules : new List<IRateLimitRule>();

    private IEnumerable<IRateLimitRule> CreateRulesFromConfig(RuleConfig config)
    {
        var rules = _ruleFactory.CreateRules(config);
        return rules;
    }
}
