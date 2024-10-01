using System;
using RateLimiter.Common;

namespace RateLimiter.Rules.TimespanSinceLastCall
{
    public record TimespanSinceLastCallRuleOptions(TimeSpan TimeSpan)
    {
    }
}
