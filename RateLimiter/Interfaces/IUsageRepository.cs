namespace RateLimiter.Interfaces
{
    public interface IUsageRepository
    {
        RequestUsage GetUsageForClient(string clientToken);
        void UpdateUsageForClient(string clientToken, RequestUsage usage);

    }
}
