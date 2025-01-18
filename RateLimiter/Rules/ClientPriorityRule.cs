using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RateLimiter.Interfaces;

public class ClientPriorityRule : IRule
{
    private readonly int _baseLimit;
    private readonly TimeSpan _timeSpan;
    private readonly Dictionary<string, int> _clientPriorityLevels;
    private readonly ConcurrentDictionary<string, (int Count, DateTime ResetTime)> _requestTracker = new();

    public ClientPriorityRule(int baseLimit, TimeSpan timeSpan, Dictionary<string, int> clientPriorityLevels)
    {
        _baseLimit = baseLimit;
        _timeSpan = timeSpan;
        _clientPriorityLevels = clientPriorityLevels;
    }

    public bool IsAllowed(string clientId, string resource)
    {
        var key = $"{clientId}:{resource}";
        var priority = _clientPriorityLevels.ContainsKey(clientId) ? _clientPriorityLevels[clientId] : 1;
        var limit = _baseLimit * priority;
        var currentTime = DateTime.UtcNow;

        if (!_requestTracker.ContainsKey(key) || currentTime >= _requestTracker[key].ResetTime)
        {
            _requestTracker[key] = (1, currentTime + _timeSpan);
            return true;
        }

        if (_requestTracker[key].Count < limit)
        {
            _requestTracker[key] = (_requestTracker[key].Count + 1, _requestTracker[key].ResetTime);
            return true;
        }

        return false;
    }
}
