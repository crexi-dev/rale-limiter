using RateLimiter.Domain.Models;
using System.Threading.Tasks;

namespace RateLimiter.Domain.Interfaces
{
    public interface IRule
    {
        Task<Models.RulesResult> ExecuteRule(RateLimiterRequest request, Configurations configurations, RateLimiterStats rateLimiterStats);
    }
}
