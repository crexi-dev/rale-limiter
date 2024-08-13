using System;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

/// <summary>
/// Defines a storage contract for default rules and policies provided
/// </summary>
public interface IRateLimiterStorage
{
    Task SetAsync<TValue>(string key, TValue value, TimeSpan lifetime, CancellationToken ct = default);
    Task SetAsync<TValue>(string key, TValue value, CancellationToken ct = default);
    Task<bool> TryGetAsync<TValue>(string key, out TValue? value, CancellationToken ct = default);
    Task<bool> RemoveAsync(string key, CancellationToken ct = default);
}
