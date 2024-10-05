using RateLimiter.Model;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RateLimiter.Data
{
    /// <summary>
    /// Data source to keep track of the request counts and the last time a client made a request.
    /// The in-memory approach used in the current code will only work for a single instance of the API. 
    /// To scale, we need to use a centralized or distributed data store to track request counts and timestamps globally across all instances of the API.
    /// </summary>
    public class RateLimiterDataStore : IRateLimiterDataStore
    {
        private readonly ConcurrentDictionary<string, ClientRateLimiterData> _clientData = new ConcurrentDictionary<string, ClientRateLimiterData>();
        private readonly object _lock = new object();

        /// <summary>
        /// In .NET, the ConcurrentDictionary<TKey, TValue> is a thread-safe collection designed for concurrent access. 
        /// It handles locking internally, ensuring that read and write operations can happen in parallel without explicit locking.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public ClientRateLimiterData GetClientData(string clientId, string resource)
        {
            var key = GetKey(clientId, resource);
            return _clientData.GetOrAdd(key, new ClientRateLimiterData
            {
                StartTime = DateTime.UtcNow,
                LastRequestTime = DateTime.UtcNow,
                RequestCount = 0
            });
        }

        /// <summary>
        /// Here, lock guarantees that the increment operation is atomic, even when multiple threads try to increment the same value.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="resource"></param>
        public void IncrementRequestCount(string clientId, string resource)
        {
            var key = GetKey(clientId, resource);

            // This lock may affect performance of the process negetively. Redis has its own increment process and distributed locks and it handles internally.
            lock (_lock)
            {
                var clientData = _clientData.GetOrAdd(key, new ClientRateLimiterData
                {
                    StartTime = DateTime.UtcNow,
                    LastRequestTime = DateTime.UtcNow,
                    RequestCount = 0
                });
                //AtomicIncrement(clientData);
                clientData.RequestCount++;
            }
        }

        private void AtomicIncrement(ClientRateLimiterData clientData)
        {
            int count = clientData.RequestCount;
            Interlocked.Increment(ref count);//didn't work
            clientData.RequestCount = count;
        }

        public void ResetClientData(string clientId, string resource)
        {
            var key = GetKey(clientId, resource);
            _clientData[key] = new ClientRateLimiterData { StartTime = DateTime.UtcNow, LastRequestTime = DateTime.UtcNow, RequestCount = 0 };
        }

        private string GetKey(string clientId, string resource) => $"{clientId}_{resource}";
    }
}
