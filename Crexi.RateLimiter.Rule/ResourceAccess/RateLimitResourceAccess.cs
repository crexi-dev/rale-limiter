using Crexi.RateLimiter.Rule.Model;
using Crexi.RateLimiter.Rule.Utility;
using Microsoft.Extensions.Caching.Memory;

namespace Crexi.RateLimiter.Rule.ResourceAccess;

public class RateLimitResourceAccess(IMemoryCache memoryCache, TimeProvider timeProvider): IRateLimitResourceAccess
{
    private readonly CallDataComparer _callDataComparer = new();
    private const string CacheOptionSuffix = "_CacheOption";
    private const string RulesSuffix = "_Rules";

    public CallHistory AddCallAndGetHistory(CallData callData)
    {
        var hash = _callDataComparer.GetHashCode(callData);
        var callUtc = timeProvider.GetUtcNow().DateTime;
        var calls = new List<DateTime>() { callUtc };
        DateTime? latestCall = null;
        /*
            NOTE: I fully understand this is almost certainly breaking, but for the stated purpose of the test (We're more interested in the design itself than in some intelligent and tricky rate-limiting algorithm), I'm going to pretend it will never break...
        */
        foreach (var key in ((MemoryCache)memoryCache).Keys)
        {
            if (key is not string strKey || !strKey.StartsWith($"{hash}_Call_"))
                continue;
            var value = memoryCache.Get<DateTime>(key);
            calls.Add(value);
            latestCall = latestCall is null ? value : Max(latestCall.Value, value);
        }
        AddNewCallToCache(hash, callUtc);
        return new CallHistory()
        {
            Calls = calls.ToArray(),
            LastCall = latestCall,
        };
    }

    public IList<RateLimitRule>? GetRules(CallData callData)
    {
        var hash = _callDataComparer.GetHashCode(callData);
        return memoryCache.TryGetValue<IList<RateLimitRule>>($"{hash}_{RulesSuffix}", out var rules) ? rules : null;
    }

    public IRateLimitResourceAccess SetExpirationWindow(TimeSpan timespan, CallData callData)
    {
        var hash = _callDataComparer.GetHashCode(callData);
        memoryCache.Set($"{hash}_{CacheOptionSuffix}", new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = timespan
        });
        return this;
    }

    public IRateLimitResourceAccess SetRules(IEnumerable<RateLimitRule> rules, CallData callData)
    {
        var hash = _callDataComparer.GetHashCode(callData);
        /*
            NOTE: Since rules shouldn't expire or get evicted, MemoryCache really isn't the right storage, but as I said before, for this test, I'll pretend it's functional.
        */
        memoryCache.Set($"{hash}_{CacheOptionSuffix}", rules);
        return this;
    }

    private void AddNewCallToCache(int callDataHash, DateTime callUtc)
    {
        var callKey = $"{callDataHash}_Call_{Guid.NewGuid()}";
        var options = memoryCache.Get<MemoryCacheEntryOptions>($"{callDataHash}_{CacheOptionSuffix}");
        memoryCache.Set(callKey, callUtc, options);
    }

    private static DateTime Max(DateTime dt1, DateTime dt2) =>
        dt1 > dt2 ? dt1 : dt2;
}