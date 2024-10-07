using RateLimiter.Interfaces;
using RateLimiter.Results;
using RateLimiter.Storage;
using System;

namespace RateLimiter.Rules;

/// <summary>
/// Rule that allows requests if a min time has passed between them
/// For instance, 1 request every 5 seconds
/// </summary>
public class TimeRateLimiterRule : AbstractRateLimiterRule<CountBasedStorageEntry, CountBasedRateLimiterResult>
{
    public TimeRateLimiterRule(
        IRateLimiterStorage storage,
        TimeSpan timeSpan
    ) : base(storage)
    {
        _timeSpan = timeSpan;
    }

    private readonly TimeSpan _timeSpan;

    /// <summary>
    /// Storage Key
    /// </summary>
    protected override string Key => $"{nameof(TimeRateLimiterRule)}_{AccessToken}";

    /// <summary>
    /// Gets or creates a new storage entry
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
        var timeElapsed = DateTime.UtcNow - storageEntry.LastAccessTime;
        if (timeElapsed > _timeSpan)
        {
            // Expired - reset the count and update the last request time
            storageEntry.Count = 0;
            storageEntry.LastAccessTime = DateTime.UtcNow;
        } 
        //else if there are entries and timeElapsed is less than the rule, the request is not allowed
        else if (storageEntry.Count > 0 && timeElapsed < _timeSpan)
        {
            return result; //broken rule
        }

        //otherwise, we update the counter and return success
        storageEntry.Count++;
        Storage.Set(Key, _timeSpan, storageEntry);
        result.Success = true;
        return result;
    }
}
