using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RateLimiter.Abstractions;
using RateLimiter.Exceptions;
using RateLimiter.Infrastructure;

namespace RateLimiter.Implementations;

public class RateLimiter : IRateLimiter
{
    private readonly ILogger<RateLimiter> _logger;
    private readonly IRequestsRepository _requestsRepository;
    private readonly IEnumerable<IRateLimiterRule> _rules;

    public RateLimiter(IEnumerable<IRateLimiterRule> rules, IRequestsRepository requestsRepository, ILogger<RateLimiter> logger)
    {
        _rules = rules;
        _requestsRepository = requestsRepository;
        _logger = logger;
    }

    public void LimitRequestsForToken(string token)
    {
        var previousRequests = _requestsRepository.GetPreviousRequests(token);

        try
        {
            foreach (var rule in _rules)
            {
                rule.Validate(token, previousRequests);
            }
        }
        catch (RateLimitException ex)
        {
            _logger.LogInformation("Too many Attempts for Token: {Token}; Rule: {Rule}.", token, ex.RuleType);
            throw;
        }
    }
}