using System;
using System.Threading.Tasks;

namespace RateLimiter.Stores
{
    public interface IRequestCountStore
    {
        Task<long> RequestCountSince(string id, DateTimeOffset date);
    }
}
