namespace RateLimiter.Interfaces
{
    public interface IUsageRepository
    {
        public RequestUsage GetUsageForClient(string clientToken);
        public void UpdateUsageForClient(string clientToken, RequestUsage usage);

    }
}
