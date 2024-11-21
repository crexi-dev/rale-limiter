using RateLimiter.Models.Requests;
using RulesService.Models;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public interface IRateLimiterService
    {
        Task<RateLimiterResponse> GetRateLimiterRules(RateLimiterRequest request);
        RateLimiterRules GetDefaultRules();

    }
}