using RateLimiter.Interface;
using RateLimiter.Model;
using System.Collections.Concurrent;

namespace RateLimiter.Repository
{
    public class RateLimiterDataStore : IRateLimiterRepository
    {
        private readonly ConcurrentDictionary<string, Request> _concurrentDictionary;
        private readonly object _lock = new object();

        public RateLimiterDataStore()
        {
            _concurrentDictionary = new ConcurrentDictionary<string, Request>();
        }

        public Request Update(RequestDTO requestDTO)
        {
            if (_concurrentDictionary.ContainsKey(requestDTO.CallId))
            {
                lock (_lock)
                {
                    var currentRequest = _concurrentDictionary[requestDTO.CallId];
                    currentRequest.AccessTime.Add(requestDTO.CurrentTime);
                    _concurrentDictionary[requestDTO.CallId] = currentRequest;
                    return currentRequest;
                }
            }
            var request = ConvertToRequest(requestDTO);
            _concurrentDictionary[requestDTO.CallId] = request;
            return request;
        }

        public Request Get(RequestDTO requestDTO)
        {
            if (_concurrentDictionary.ContainsKey(requestDTO.CallId))
            {
                return _concurrentDictionary[requestDTO.CallId];
            }

            var request = ConvertToRequest(requestDTO);
            _concurrentDictionary[requestDTO.CallId] = request;
            return request;
        }

        private Request ConvertToRequest(RequestDTO requestDTO)
        {            
            return new Request
            {
                CallId = requestDTO.CallId,
                AccessTime = new List<DateTime> { requestDTO.CurrentTime }
            };
        }
    }
}