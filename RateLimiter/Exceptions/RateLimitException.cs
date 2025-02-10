using System;

namespace RateLimiter.Exceptions;

public class RateLimitException(string message, string ruleType) : Exception(message)
{
    public string RuleType => ruleType;
}