using System.Collections.Generic;

namespace RateLimiter;
public static class DictionaryExtensions
{
    public static bool TryGetValue<T>(this IDictionary<string, object> dictionary, string key, out T value)
    {
        if (dictionary.TryGetValue(key, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default!;
        return false;
    }
}
