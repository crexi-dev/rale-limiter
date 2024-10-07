using RateLimiter.Interfaces;
using RateLimiter.Results;
using RateLimiter.Storage;
using System;

namespace RateLimiter.Rules;

/// <summary>
/// Rule that allows a maximum amount of requests on a fixed window
/// For instance, 5 requests in 1 minute
/// </summary>
public class RequestRateLimiterRule : AbstractRateLimiterRule<CountBasedStorageEntry, CountBasedRateLimiterResult>
{
    public RequestRateLimiterRule(
        IRateLimiterStorage storage,
        int maxRequests,
        TimeSpan timeSpan
    ) : base(storage)
    {
        _maxRequests = maxRequests;
        _timeSpan = timeSpan;
    }

    private readonly int _maxRequests;
    private readonly TimeSpan _timeSpan;

    /// <summary>
    /// Storage key
    /// </summary>
    protected override string Key => $"{nameof(RequestRateLimiterRule)}_{AccessToken}";

    /// <summary>
    /// Implementation of new storage entry
    /// </summary>
    /// <returns></returns>
    protected override CountBasedStorageEntry GetOrCreateStorageEntry()
    {
        return (CountBasedStorageEntry)Storage.GetOrCreate(Key, _timeSpan, new CountBasedStorageEntry
        {
            Count = 0,
            LastAccessTime = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Retry after calculation
    /// </summary>
    /// <param name="rateLimitResult"></param>
    /// <returns></returns>
    protected override double CaculateRetryAfter(CountBasedRateLimiterResult rateLimitResult)
    {
        if (rateLimitResult.StorageEntry != null)
        {
            var remainingTime = _timeSpan - (DateTime.UtcNow - rateLimitResult.StorageEntry.LastAccessTime);
            return Math.Max(remainingTime.TotalSeconds, 0);
        }

        return 0;
    }

    /// <summary>
    /// Implementation of the rule
    /// </summary>
    /// <returns></returns>
    public override CountBasedRateLimiterResult IsRequestAllowed()
    {
        //gets or create a new entry in the storage
        var storageEntry = GetOrCreateStorageEntry();

        var result = new CountBasedRateLimiterResult
        {
            StorageEntry = storageEntry
        };

        //to avoid some latency from the IMemoryCache to expire an entry,
        //we reset it if the right amount of time has already passed
        if (DateTime.UtcNow - storageEntry.LastAccessTime > _timeSpan)
        {
            // Expired - reset the count and update the last request time
            storageEntry.Count = 0;
            storageEntry.LastAccessTime = DateTime.UtcNow;
        }

        //if the amount of accesses went beyond to the max requests, the request is not allowed
        if (storageEntry.Count >= _maxRequests)
        {
            return result;
        }

        //otherwise, we update the counter and return success
        storageEntry.Count++;
        Storage.Set(Key, _timeSpan, storageEntry);

        result.Success = true;

        return result;
    }
}
