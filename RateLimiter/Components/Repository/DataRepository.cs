using RateLimiter.Components.Repository.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Components.Repository
{
    /// <summary>
    /// This is a sample of a class that keeps the state, this should be something fast and shared like redis. 
    /// This example is just a concurrent dictionary 
    /// </summary>
    public class DataRepository : IDataRepository
    {
        private readonly ConcurrentDictionary<string, RateLimitingBaseDataRepositoryState> _repository = new ConcurrentDictionary<string, RateLimitingBaseDataRepositoryState>();

        public Task<T?> GetStateAsync<T>(string key) where T : RateLimitingBaseDataRepositoryState
        {
            var result = _repository.GetValueOrDefault(key) as T;

            return Task.FromResult(result);
        }



        public Task SaveStateAsync<T>(string key, T state) where T : RateLimitingBaseDataRepositoryState
        {
            _repository.AddOrUpdate(key, state, (name, oldValue) => state);

            return Task.CompletedTask;
        }
    }
}
