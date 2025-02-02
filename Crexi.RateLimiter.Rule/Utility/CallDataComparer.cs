using Crexi.RateLimiter.Rule.Model;

namespace Crexi.RateLimiter.Rule.Utility;

public class CallDataComparer: IEqualityComparer<CallData>
{
    public bool Equals(CallData? x, CallData? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.RegionId == y.RegionId && x.TierId == y.TierId && x.ClientId == y.ClientId && x.Resource == y.Resource;
    }

    public int GetHashCode(CallData obj)
    {
        return HashCode.Combine(obj.RegionId, obj.TierId, obj.ClientId, obj.Resource);
    }
}