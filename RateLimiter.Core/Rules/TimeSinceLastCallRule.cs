using System;
using System.Collections.Generic;
using RateLimiter.Core.Configuration.Contracts;

namespace RateLimiter.Core.Rules;

public class TimeSinceLastCallRule : IRateLimitRule
{
    private readonly TimeSpan _requiredDelay;
    private readonly ISystemTime _systemTime;
    private readonly Dictionary<string, Dictionary<string, DateTime>> _lastRequestTimes = new();
    private readonly ILogger _logger;
    public TimeSinceLastCallRule(TimeSpan requiredDelay, ISystemTime systemTime = null!, ILogger logger = null!)
    {
        if (requiredDelay.Ticks < 0)
            throw new ArgumentException("Delay cannot be negative", nameof(requiredDelay));
        _requiredDelay = requiredDelay;
        _systemTime = systemTime;
        _logger = logger;
    }

    public bool IsAllowed(string clientToken, string resourceKey)
    {
        try
        {
            lock (_lastRequestTimes)
            {
                var currentTime = _systemTime.GetCurrentUtcTime();
                
                if (!_lastRequestTimes.TryGetValue(clientToken, out var resourceTimes) ||
                    !resourceTimes.TryGetValue(resourceKey, out var lastTime))
                {
                    return true;
                }

                var timeSinceLastCall = currentTime - lastTime;
                _logger.LogInformation($"Client {clientToken} last call: " +
                                       $"{timeSinceLastCall.TotalSeconds}s ago (required {_requiredDelay.TotalSeconds}s)");
                
                return timeSinceLastCall >= _requiredDelay;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in TimeSinceLastCallRule.IsAllowed for {clientToken}", ex);
            return false;
        }
    }

    public void RecordRequest(string clientToken, string resourceKey)
    {
        lock (_lastRequestTimes)
        {
            if (!_lastRequestTimes.TryGetValue(clientToken, out var resourceTimes))
            {
                resourceTimes = new Dictionary<string, DateTime>();
                _lastRequestTimes[clientToken] = resourceTimes;
            }

            resourceTimes[resourceKey] = _systemTime.GetCurrentUtcTime();
        }
    }
}