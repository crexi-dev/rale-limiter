using RateLimiter.Data.Models;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    public interface ILimiterService
    {
        public Task<bool> AllowAccess(Request request);
    }
}
