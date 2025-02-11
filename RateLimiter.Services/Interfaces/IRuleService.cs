namespace RateLimiter.Services.Interfaces
{
    public interface IRuleService
    {
        Task DeleteOldRequestLogs(int pastHours);

        Task<bool> HasRateLimitExceeded(string token, string resourceName);
    }
}
