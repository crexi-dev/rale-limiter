namespace Crexi.RateLimiter.Interfaces;

public interface IRateLimitEngine
{
    public RateLimitEngineResult CheckClientRequest(
        ClientRequest request, 
        IClientRequestTracker requestTracker,
        IList<RateLimitPolicy>? policies,
        ILogger logger);
}