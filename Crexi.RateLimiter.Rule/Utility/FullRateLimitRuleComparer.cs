using Crexi.RateLimiter.Rule.Model;

namespace Crexi.RateLimiter.Rule.Utility;

public class FullRateLimitRuleComparer : IEqualityComparer<RateLimitRule>
{
    public bool Equals(RateLimitRule? x, RateLimitRule? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.RegionId == y.RegionId && x.TierId == y.TierId && x.ClientId == y.ClientId &&
               x.Resource == y.Resource && x.Timespan == y.Timespan && x.MaxCallCount == y.MaxCallCount &&
               x.EvaluationType == y.EvaluationType && x.OverrideResponseCode == y.OverrideResponseCode &&
               Nullable.Equals(x.EffectiveWindowStartUtc, y.EffectiveWindowStartUtc) &&
               Nullable.Equals(x.EffectiveWindowEndUtc, y.EffectiveWindowEndUtc);
    }

    public int GetHashCode(RateLimitRule obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.RegionId);
        hashCode.Add(obj.TierId);
        hashCode.Add(obj.ClientId);
        hashCode.Add(obj.Resource);
        hashCode.Add(obj.Timespan);
        hashCode.Add(obj.MaxCallCount);
        hashCode.Add((int)obj.EvaluationType);
        hashCode.Add(obj.OverrideResponseCode);
        hashCode.Add(obj.EffectiveWindowStartUtc);
        hashCode.Add(obj.EffectiveWindowEndUtc);
        return hashCode.ToHashCode();
    }
}