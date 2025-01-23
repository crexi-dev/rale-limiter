namespace Crexi.RateLimiter.Interfaces;

public interface IClientRequestTracker
{ 
    public void AddRequest(ClientRequest request);
    public void EndRequest(string clientId, string requestId);
    public ConcurrentQueue<ClientRequest> GetRequestQueue(string clientId);
    public long GetActiveRequestCount(string clientId);
}