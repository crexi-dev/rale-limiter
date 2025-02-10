using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Rules;

/// <summary>
/// Decorator that wraps <see cref="IRateLimitRule"/> and allows adding functionality.
/// </summary>
public abstract class RateLimitRuleDecorator : IRateLimitRule
{
    protected IRateLimitRule _rateLimitRule;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitRuleDecorator"/>.
    /// </summary>
    /// <param name="rateLimitRule"></param>
    protected RateLimitRuleDecorator(IRateLimitRule rateLimitRule)
    {
        _rateLimitRule = rateLimitRule;
    }

    /// <summary>
    /// Evaluates the decorated rule.
    /// </summary>
    /// <param name="context"><see cref="RateLimitContext"/>.</param>
    /// <returns><see cref="RateLimitResponse"/>.</returns>
    public abstract RateLimitResponse Evaluate(RateLimitContext context);
}