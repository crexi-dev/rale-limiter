using Crexi.RateLimiter.Rule.Model;

namespace Crexi.RateLimiter.Rule.Utility;

/// <summary>
/// Compares rules on non-variable identifying fields
/// </summary>
public class UpdateRateLimitRuleComparer : IEqualityComparer<RateLimitRule>
{
    /*
        NOTE: this is really the logic for what can get updated.  Namely:
            Timespan, MaxCallCount, OverrideResponseCode, EffectiveWindowStartUtc, EffectiveWindowEndUtc
            Arguable this hides the logic a bit, but for what we're doing here I'm running with it.
    */
    public bool Equals(RateLimitRule? x, RateLimitRule? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.RegionId == y.RegionId && x.TierId == y.TierId && x.ClientId == y.ClientId &&
               x.Resource == y.Resource && x.EvaluationType == y.EvaluationType;
    }

    public int GetHashCode(RateLimitRule obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.RegionId);
        hashCode.Add(obj.TierId);
        hashCode.Add(obj.ClientId);
        hashCode.Add(obj.Resource);
        hashCode.Add((int)obj.EvaluationType);
        return hashCode.ToHashCode();
    }
}