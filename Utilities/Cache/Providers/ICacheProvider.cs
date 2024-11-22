using Cache.Models;
namespace Cache.Providers;

public interface ICacheProvider
{
    Task Set<T>(string key, T value, CacheOptions? cacheOptions);
    Task<T?> Get<T>(string key);
    Task Remove(string key);
    void Dispose();

}
