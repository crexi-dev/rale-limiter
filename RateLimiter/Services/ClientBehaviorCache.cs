using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace RateLimiter.Services
{
    public class ClientBehaviorCache(IMemoryCache memoryCache, IConfiguration configuration)
    {
        public void Add(string apiKey, DateTime requestTime)
        {
            // Record the request time if the cache contains an entry for this API key.
            bool hasEntry = memoryCache.TryGetValue(apiKey, out var priorActivities);
            if (hasEntry)
            {
                if (priorActivities is ClientActivityTracker clientTracker)
                {
                    clientTracker.RecordActivity(requestTime);
                    return;
                }
            }

            // Otherwise use the factory method to configure entry expiration.
            memoryCache.GetOrCreate(apiKey, entry =>
            {
                int expirationWindow = Convert.ToInt32(configuration["RateLimiting:CacheExpirationMinutes"]);
                entry.SlidingExpiration = TimeSpan.FromMinutes(expirationWindow);

                return new ClientActivityTracker(requestTime);
            });
        }

        public ClientActivityTracker Get(string apiKey)
        {
            if (memoryCache.TryGetValue(apiKey, out var priorActivities))
            {
                if (priorActivities is ClientActivityTracker clientTracker)
                {
                    return clientTracker;
                }
            }

            string errorMessage = "Logic Error: The API key should always be present and refer to a non-null value when this is called.";
            throw new InvalidOperationException(errorMessage);
        }

        public class ClientActivityTracker : ConcurrentQueue<DateTime>
        {
            public ClientActivityTracker(DateTime timeStamp) =>
                RecordActivity(timeStamp);

            public void RecordActivity(DateTime timeStamp)
            {
                Enqueue(timeStamp);
            }
        }
    }
}
