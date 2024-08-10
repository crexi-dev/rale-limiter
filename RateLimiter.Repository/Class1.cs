using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Interface;
using RateLimiter.Model;

namespace RateLimiter.Repository
{
    public class RateLimiterDataStore : IRateLimiterRepository
    {
        private readonly IMemoryCache _memoryCache;
        public RateLimiterDataStore(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }      
        public void Add(Request request)
        {
            if (!_memoryCache.TryGetValue(request, out Request existingRequest))
            {
                _memoryCache.Set(request.CallId, request);
            }
            existingRequest.AccessTime.Add(request.CurrentTime);
            

        }
        public Request Get(string callId)
        {            
            return _memoryCache.Get<Request>(callId);
        }
    }
}