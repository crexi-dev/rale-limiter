using RateLimiter.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter.Rules
{
    public class BurstRule : IRateLimitRule
    {
        private readonly int _maxBurstRequests;
        private readonly TimeSpan _burstWindow;
        private readonly TimeSpan _cooldownPeriod;
        private readonly Dictionary<string, (DateTime, int, bool)> _clientRequestTracker;

        public BurstRule(int maxBurstRequests, TimeSpan burstWindow, TimeSpan cooldownPeriod)
        {
            _maxBurstRequests = maxBurstRequests;
            _burstWindow = burstWindow;
            _cooldownPeriod = cooldownPeriod;
            _clientRequestTracker = new Dictionary<string, (DateTime, int, bool)>();
        }

        public bool IsRequestAllowed(string clientId)
        {
            if (!_clientRequestTracker.ContainsKey(clientId))
            {
                _clientRequestTracker[clientId] = (DateTime.Now, 1, false);
                return true;
            }

            var (lastRequestTime, requestCount, isInCooldown) = _clientRequestTracker[clientId];
            var now = DateTime.Now;

            // Check if in cooldown
            if (isInCooldown && (now - lastRequestTime) < _cooldownPeriod)
            {
                return false; // Still in cooldown
            }
            else if (isInCooldown && (now - lastRequestTime) >= _cooldownPeriod)
            {
                // Exit cooldown
                _clientRequestTracker[clientId] = (now, 1, false);
                return true;
            }

            // Burst window logic
            if ((now - lastRequestTime) > _burstWindow)
            {
                _clientRequestTracker[clientId] = (now, 1, false); // Reset burst window
                return true;
            }
            else if (requestCount < _maxBurstRequests)
            {
                _clientRequestTracker[clientId] = (lastRequestTime, requestCount + 1, false);
                return true;
            }
            else
            {
                // Enter cooldown
                _clientRequestTracker[clientId] = (now, requestCount, true);
                return false;
            }
        }
    }


}
