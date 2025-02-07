using System.Threading.Tasks;

namespace RateLimiter.Domain.Interfaces
{
    public interface IRuleRunner
    {
        Task<Models.RulesResult> RunRules(Models.RateLimiterRequest request, Models.Configurations configurations);
    }
}
