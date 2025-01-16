using System;

namespace RateLimiter;
public static class StringComparisonDefaults
{
    public static readonly StringComparer DefaultStringComparer = StringComparer.InvariantCultureIgnoreCase;
    public static readonly StringComparison DefaultStringComparison = StringComparison.InvariantCultureIgnoreCase;
}
