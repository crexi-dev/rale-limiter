namespace Crexi.RateLimiter.Logic;

/// <summary>
/// In-memory tracking of requests. Active requests are separate from history queue
/// </summary>
public class ClientRequestTracker : IClientRequestTracker
{
    private ConcurrentDictionary<string, ActiveRequestMonitor> _allActiveRequestsMap = new();
    private ConcurrentDictionary<string, ConcurrentQueue<ClientRequest>> _allRequestsMap = new();

    /// <summary>
    /// Main entrypoint to log a request. Also for tracking of an active request 
    /// </summary>
    /// <param name="request"></param>
    public void AddRequest(ClientRequest request)
    {
        _allActiveRequestsMap.TryGetValue(request.ClientId, out ActiveRequestMonitor? activeRequests);
        if (activeRequests == null)
        {
            activeRequests = new ActiveRequestMonitor();
            _allActiveRequestsMap.TryAdd(request.ClientId, activeRequests);
        }
        activeRequests.AddRequest(request.RequestId, request.RequestTime);
        
        _allRequestsMap.TryGetValue(request.ClientId, out ConcurrentQueue<ClientRequest>? requestQueue);
        if (requestQueue == null)
        {
            requestQueue = new ConcurrentQueue<ClientRequest>();
            _allRequestsMap.TryAdd(request.ClientId, requestQueue);
        }
        requestQueue.Enqueue(request);
    }

    /// <summary>
    /// Calls to EndRequest are required for concurrency rate limiting.
    /// Middleware does not need to make this extra call if concurrency limits are not needed
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="requestId"></param>
    public void EndRequest(string clientId, string requestId)
    {
        _allActiveRequestsMap.TryGetValue(clientId, out ActiveRequestMonitor? activeRequests);
        activeRequests?.RemoveRequest(requestId);
    }

    /// <summary>
    /// Used for concurrency checks. Returns a count of active requests for a given client ID 
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns>Count of active requests</returns>
    public long GetActiveRequestCount(string clientId)
    {
        _allActiveRequestsMap.TryGetValue(clientId, out ActiveRequestMonitor? activeRequests);
        return activeRequests?.ActiveRequestCount ?? 0;
    }

    /// <summary>
    /// Returns queue of requests made for a specific client ID
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns>Concurrent collection instead of interface</returns>
    public ConcurrentQueue<ClientRequest> GetRequestQueue(string clientId)
    {
        _allRequestsMap.TryGetValue(clientId, out ConcurrentQueue<ClientRequest>? requestQueue);
        return requestQueue ?? new();
    }
}