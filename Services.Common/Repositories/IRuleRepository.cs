using Services.Common.Configurations;

namespace Services.Common.Repositories;

public interface IRuleRepository
{
    Task<IList<RuleConfig>> GetRuleConfigsAsync();
}