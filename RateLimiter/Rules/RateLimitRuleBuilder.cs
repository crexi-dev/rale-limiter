using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Model;

namespace RateLimiter.Rules
{
    public class RateLimitRuleBuilder : IRateLimitRuleBuilder
    {
        private bool _allowRequest = true;
        private readonly IMemoryCache _memoryCache;
        private List<IRateLimitRule> _rules = new List<IRateLimitRule>();
        private ClientModel _clientData;
        private string _resourceName;

        public RateLimitRuleBuilder(IMemoryCache memoryCache, ClientModel clientData, string resourceName)
        {
            _memoryCache = memoryCache;
            _clientData = clientData;
            _resourceName = resourceName;
        }

        public IRateLimitRuleBuilder Build()
        {
            foreach (var rule in _rules)
            {
                if (!_allowRequest)
                {
                    break;
                }

                _allowRequest = rule.IsRequestAllowed(_clientData, _resourceName, _memoryCache);
            }

            return this;
        }

        public IRateLimitRuleBuilder WithRequestCountRule(int maxRequests, TimeSpan timeSpan)
        {
            var rule = new RequestCountRule(maxRequests, timeSpan);
            _rules.Add(rule); 

            return this;
        }

        public IRateLimitRuleBuilder WithTimeSinceLastCallRule(TimeSpan minInterval)
        {
            var rule = new TimeSinceLastCallRule(minInterval);
            _rules.Add(rule);

            return this;
        }

        public IRateLimitRuleBuilder ApplyRule(bool condition, IRateLimitRule rule)
        {
            if (condition)
            {
                _rules.Add(rule);
            }

            return this;
        }

        public bool IsAllowed()
        {
            return _allowRequest;
        }
    }
}