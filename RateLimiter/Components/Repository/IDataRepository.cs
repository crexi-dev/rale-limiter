using RateLimiter.Components.Repository.Models;
using System.Threading.Tasks;

namespace RateLimiter.Components.Repository
{
    public interface IDataRepository
    {
        public Task<T?> GetStateAsync<T>(string key) where T: RateLimitingBaseDataRepositoryState;

        public Task SaveStateAsync<T>(string key, T state) where T : RateLimitingBaseDataRepositoryState;
    }
}
