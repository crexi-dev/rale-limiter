using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    public interface IRateLimiter
    {
        Task ClearOldRequestLogs(int pastHours);

        Task<bool> IsRequestAllowed(string token, string resourceName);
    }
}
