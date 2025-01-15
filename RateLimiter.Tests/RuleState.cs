using System;

namespace RateLimiter;

public class RuleState
{
    private readonly string _scope;
    private readonly int _requestCount;
    private readonly DateTime? _lastRequestTime;

    public RuleState(string scope, int requestCount, DateTime? lastRequestTime = null)
    {
        _scope = scope;
        _requestCount = requestCount;
        _lastRequestTime = lastRequestTime;
    }
}
