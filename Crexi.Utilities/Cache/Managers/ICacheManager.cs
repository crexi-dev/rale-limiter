using Crexi.Cache.Models;

namespace Crexi.Utilities.Cache.Managers
{
    public interface ICacheManager
    {
        void Dispose();
        Task<T?> Get<T>(string key);
        Task<List<string>> GetKeys(string keyPattern);
        Task<Dictionary<string, T>> GetValues<T>(string keyPattern);
        Task Remove(string key);
        Task Set<T>(string key, T value, CacheOptions cacheOptions);
    }
}