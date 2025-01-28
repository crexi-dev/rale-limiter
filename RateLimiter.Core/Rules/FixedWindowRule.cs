using System;
using System.Collections.Generic;
using RateLimiter.Core.Configuration;
using RateLimiter.Core.Configuration.Contracts;

namespace RateLimiter.Core.Rules;
public class FixedWindowRule : IRateLimitRule
{
    private readonly int _maxRequests;
    private readonly TimeSpan _windowDuration;
    private readonly ISystemTime _systemTime;
    private readonly ILogger _logger;

    private readonly Dictionary<string, Dictionary<string, (int Count, DateTime WindowStart)>> _clientResourceData = new();

    public FixedWindowRule(int maxRequests, TimeSpan windowDuration,  ISystemTime systemTime = null, ILogger logger = null)
    {
        if (maxRequests <= 0)
            throw new ArgumentException("Max requests must be positive", nameof(maxRequests));
        _maxRequests = maxRequests;
        _windowDuration = windowDuration;
        _logger = logger;
        _systemTime = systemTime;
    }

    public bool IsAllowed(string clientToken, string resourceKey)
    {
        if (string.IsNullOrEmpty(clientToken))
            throw new ArgumentNullException(nameof(clientToken));
        
        if (string.IsNullOrEmpty(resourceKey))
            throw new ArgumentNullException(nameof(resourceKey));

        try
        {
            lock (_clientResourceData)
            {
                var currentTime = _systemTime.GetCurrentUtcTime();
                var data = GetOrInitializeData(clientToken, resourceKey, currentTime);
                
                _logger.LogInformation($"Client {clientToken} resource {resourceKey}: " +
                                       $"{data.Count}/{_maxRequests} requests in current window");
                
                return data.Count < _maxRequests;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in FixedWindowRule.IsAllowed for {clientToken}", ex);
            return false;
        }
    }

    public void RecordRequest(string clientToken, string resourceKey)
    {
        lock (_clientResourceData)
        {
            var currentTime = _systemTime.GetCurrentUtcTime();
            var data = GetOrInitializeData(clientToken, resourceKey, currentTime);
            data.Count++;
            UpdateData(clientToken, resourceKey, data.Count, data.WindowStart);
        }
    }

    private (int Count, DateTime WindowStart) GetOrInitializeData(string clientToken, string resourceKey, DateTime currentTime)
    {
        if (!_clientResourceData.TryGetValue(clientToken, out var resourceData))
        {
            resourceData = new Dictionary<string, (int, DateTime)>();
            _clientResourceData[clientToken] = resourceData;
        }

        if (!resourceData.TryGetValue(resourceKey, out var data))
        {
            data = (0, currentTime);
            resourceData[resourceKey] = data;
        }

        if (currentTime - data.WindowStart > _windowDuration)
        {
            data = (0, currentTime);
            resourceData[resourceKey] = data;
        }

        return data;
    }

    private void UpdateData(string clientToken, string resourceKey, int count, DateTime windowStart)
    {
        _clientResourceData[clientToken][resourceKey] = (count, windowStart);
    }
}