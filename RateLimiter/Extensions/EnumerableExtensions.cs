using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Extensions;

internal static class EnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<Task<T>> source)
    {
        foreach (var task in source)
        {
            yield return await task;
        }
    }
}
