namespace Crexi.RateLimiter.Logic;

public interface IRateLimitEngine
{
    public RateLimitEngineResult CheckClientRequest(
        ClientRequest request, 
        ConcurrentQueue<ClientRequest> requests,
        List<RateLimitPolicySettings>? policies);
}