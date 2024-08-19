using Microsoft.Extensions.Caching.Memory;

namespace RateLimiter.Interfaces
{
    public interface IMemoryCacheService
    {
        /// <summary>
        /// Gets object stored in cache by its unique key
        /// </summary>
        /// <typeparam name="T">Type of cached object</typeparam>
        /// <param name="key">Unique key of cached object</param>
        /// <returns>Object by given key (Null if not found or expired)</returns>
        public T Get<T>(string key);

        /// <summary>
        /// Stored object in cache using unique key
        /// </summary>
        /// <typeparam name="T">Type of cached object to be stored</typeparam>
        /// <param name="key">Unique key of object to be stored</param>
        /// <param name="value">Object to be stored</param>
        /// <param name="expireAfterSeconds">Duration in seconds after which object will be expired</param>
        /// <returns>Cached object itself</returns>
        public T Set<T>(string key, T value, MemoryCacheEntryOptions cacheOptions);

        public T Set<T>(string key, T value);

        /// <summary>
        /// Removes object from cache using unique key
        /// </summary>
        /// <param name="key">Unique key of object to be removed</param>
        public void Remove(string key);
    }
}
