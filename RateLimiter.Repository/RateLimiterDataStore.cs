using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Interface;
using RateLimiter.Model;
using System.Collections.Concurrent;

namespace RateLimiter.Repository
{
    public class RateLimiterDataStore : IRateLimiterRepository
    {
        private readonly ConcurrentDictionary<string, Request> _concurrentDictionary;
        public RateLimiterDataStore(IMemoryCache memoryCache)
        {
            _concurrentDictionary = new ConcurrentDictionary<string, Request>();
        }      
        public Request Update(RequestDTO requestDTO)
        {
            if (_concurrentDictionary.ContainsKey(requestDTO.CallId))
            { 
                var currentRequest = _concurrentDictionary[requestDTO.CallId];
                currentRequest.AccessTime.Add(requestDTO.CurrentTime);
                _concurrentDictionary[requestDTO.CallId] = currentRequest;
                return currentRequest;
            }
            var request = (Request)requestDTO;
            _concurrentDictionary[requestDTO.CallId] = request;
            return request;
        }
        public Request Get(RequestDTO requestDTO)
        {
            if (_concurrentDictionary.ContainsKey(requestDTO.CallId))
            {
                return _concurrentDictionary[requestDTO.CallId];
            }

            var request = (Request)requestDTO;
            _concurrentDictionary[requestDTO.CallId] = request;
            return request;
        }
    }
}