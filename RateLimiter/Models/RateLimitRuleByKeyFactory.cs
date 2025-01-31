using System;
using RateLimiter.Rules;

namespace RateLimiter.Models;
public class RateLimitRuleByKeyFactory<TKey>
{
    public TKey Key { get; set; }
    public Func<TKey, IRateLimitRule> LimitRule { get; set; }
}
