using System;
using System.Collections.Generic;

namespace RateLimiter;

public class RateLimitRequest
{
    public string Scope { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();

    public RateLimitRequest(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            throw new ArgumentException($"Argument {nameof(scope)} cannot be null, empty, or only whitespace.");
        }

        Scope = scope;
    }
}
