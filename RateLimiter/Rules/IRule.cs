using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    public interface IRule
    {
        string Name { get; }
        Task<bool> Allow(Client caller);
    }
}
