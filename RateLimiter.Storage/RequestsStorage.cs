using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Contracts;
using RateLimiter.Enums;
using RateLimiter.Models;

namespace RateLimiter.Storage;

public class RequestsStorage(IMemoryCache cache) : IRequestsStorage
{
    public List<Request> Get(Guid id) => cache.Get<List<Request>>(id) ?? [];
    
    public void Add(Request request)
    { 
        var existingRequests = cache.GetOrCreate(request.Id, _ => new List<Request>());
        existingRequests?.Add(request);
        cache.Set(request.Id, existingRequests);
    }
    
    public void RemoveOldRequests(Guid id, RegionType regionType, TimeSpan interval)
    {
        if (!cache.TryGetValue(id, out List<Request>? requests)) 
        {
            return;
        }

        var actualRequests = requests?
            .Where(request => DateTime.UtcNow - request.DateTime <= interval && request.RegionType == regionType)
            .ToList();
        
        cache.Set(id, actualRequests);
    }
}