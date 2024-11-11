using Services.Common.Configurations;

namespace Services.Common.Repositories;

public class InMemoryRuleRepository : IRuleRepository
{
    private readonly IList<RuleConfig> _ruleConfigs;
    
    public InMemoryRuleRepository(IList<RuleConfig> configs)
    {
        _ruleConfigs = configs;
    }
    
    public Task<IList<RuleConfig>> GetRuleConfigsAsync()
    {
        return Task.FromResult(_ruleConfigs);
    }
}