using System.Collections.Concurrent;

namespace RateLimiter.DataStore
{
    //this is only for demonstration in real world scenario will be used redis or other data store
    public static class Cashing
    {
        private static ConcurrentDictionary<string, string> Store = new();

        public static string? Get(string key) {
            Store.TryGetValue(key, out string? temp);
            return temp;
        }

        public static void Set(string key, string value) {
            Store.AddOrUpdate(key, value, (existingKey, existingValue) => value);
        }
        public static void Clear() { Store.Clear(); }
    }
}
