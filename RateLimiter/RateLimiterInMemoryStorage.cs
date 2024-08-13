using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

internal class RateLimiterInMemoryStorage : IRateLimiterStorage, IDisposable
{
    private readonly ConcurrentDictionary<string, CacheItem> storage = new();

    private static readonly TimeSpan defaultLifeTime = TimeSpan.FromSeconds(5);

    private readonly Timer cleanupTimer;
    private static readonly int cleanupInterval = 5000;

    private bool disposed = false;

    private readonly JsonSerializerOptions options = new()
    {
        IncludeFields = true,
    };

    public RateLimiterInMemoryStorage()
    {
        cleanupTimer = new(CleanupExpiredItems, null, cleanupInterval, cleanupInterval);
    }

    public Task SetAsync<TValue>(string key, TValue value, TimeSpan lifetime, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(disposed, nameof(RateLimiterInMemoryStorage));
        
        var expirationTime = DateTime.UtcNow.Add(lifetime);
        var cacheItem = new CacheItem(JsonSerializer.Serialize(value, options), expirationTime);

        storage.AddOrUpdate(key, cacheItem, (_, _) => cacheItem);

        return Task.CompletedTask;
    }

    public Task SetAsync<TValue>(string key, TValue value, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(disposed, nameof(RateLimiterInMemoryStorage));

        return SetAsync(key, value, defaultLifeTime, ct);
    }

    public Task<bool> TryGetAsync<TValue>(string key, out TValue? value, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(disposed, nameof(RateLimiterInMemoryStorage));

        if (storage.TryGetValue(key, out var cacheItem) && cacheItem.ExpirationTime > DateTime.UtcNow)
        {
            value = JsonSerializer.Deserialize<TValue>(cacheItem.Value, options);
            return Task.FromResult(true);
        }
        value = default;
        return Task.FromResult(false);
    }

    public Task<bool> RemoveAsync(string key, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(disposed, nameof(RateLimiterInMemoryStorage));

        return Task.FromResult(storage.TryRemove(key, out _));
    }

    private void CleanupExpiredItems(object? state)
    {
        foreach (var key in storage.Keys)
        {
            if (storage.TryGetValue(key, out var cacheItem) && cacheItem.ExpirationTime <= DateTime.UtcNow)
            {
                storage.TryRemove(key, out _);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                cleanupTimer?.Dispose();
            }

            disposed = true;
        }
    }

    private class CacheItem(string value, DateTime expirationTime)
    {
        public string Value { get; } = value;
        public DateTime ExpirationTime { get; } = expirationTime;
    }
}