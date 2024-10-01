using System;
using RateLimiter.Common;

namespace RateLimiter.Rules.CountPerTimespan
{
    public record CountPerTimespanRuleOptions(int MaxCount, TimeSpan TimeSpan)
    {
    }
}
