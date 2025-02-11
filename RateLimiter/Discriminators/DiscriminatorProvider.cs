using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Discriminators
{
    public class DiscriminatorProvider : IProvideDiscriminatorValues
    {
        private readonly ILogger<DiscriminatorProvider> _logger;
        private readonly IServiceProvider _serviceProvider;

        private readonly ConcurrentDictionary<string, IProvideADiscriminator> _discriminators = new();

        public DiscriminatorProvider(
            ILogger<DiscriminatorProvider> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Dictionary<string, (bool, string)> GetDiscriminatorValues(
            HttpContext context,
            IEnumerable<IDefineARateLimitRule> rules)
        {
            // TODO: These values should likely be cached in the caller

            var results = new Dictionary<string, (bool, string)>();

            // for each rule in here, we need to generate the discriminator value
            foreach (var rule in rules)
            {
                switch (rule.Discriminator)
                {
                    case LimiterDiscriminator.QueryString:
                        results.Add(rule.Name, GetQuerystringValue(context, rule));
                        break;
                    case LimiterDiscriminator.RequestHeader:
                        results.Add(rule.Name, GetRequestHeaderValue(context, rule));
                        break;
                    case LimiterDiscriminator.IpAddress:
                        results.Add(rule.Name, GetIpAddressValue(context, rule));
                        break;
                    case LimiterDiscriminator.Custom:
                        results.Add(rule.Name, GetCustomValue(_serviceProvider, context, rule));
                        break;
                    case LimiterDiscriminator.GeoLocation:
                    case LimiterDiscriminator.IpSubNet:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return results;
        }

        // TODO: Refactor to generic
        private (bool IsMatch, string MatchValue) GetCustomValue(IServiceProvider serviceProvider, HttpContext context, IDefineARateLimitRule rule)
        {
            IProvideADiscriminator discriminator;

            if (!_discriminators.TryGetValue(rule.Name, out var value))
            {
                discriminator = serviceProvider.GetRequiredKeyedService<IProvideADiscriminator>(rule.CustomDiscriminatorName);
                _discriminators.TryAdd(rule.Name, discriminator);
            }
            else
            {
                discriminator = value;
            }

            return discriminator.GetDiscriminator(context, rule);
        }

        private (bool IsMatch, string MatchValue) GetIpAddressValue(HttpContext context, IDefineARateLimitRule rule)
        {
            IProvideADiscriminator discriminator;

            if (!_discriminators.TryGetValue(rule.Name, out var value))
            {
                discriminator = new IpAddressDiscriminator();
                _discriminators.TryAdd(rule.Name, discriminator);
            }
            else
            {
                discriminator = value;
            }

            return discriminator.GetDiscriminator(context, rule);
        }

        private (bool IsMatch, string MatchValue) GetRequestHeaderValue(HttpContext context, IDefineARateLimitRule rule)
        {
            IProvideADiscriminator discriminator;

            if (!_discriminators.TryGetValue(rule.Name, out var value))
            {
                discriminator = new RequestHeaderDiscriminator();
                _discriminators.TryAdd(rule.Name, discriminator);
            }
            else
            {
                discriminator = value;
            }

            return discriminator.GetDiscriminator(context, rule);
        }

        private (bool IsMatch, string MatchValue) GetQuerystringValue(HttpContext context, IDefineARateLimitRule rule)
        {
            IProvideADiscriminator discriminator;

            if (!_discriminators.TryGetValue(rule.Name, out var value))
            {
                discriminator = new QueryStringDiscriminator();
                _discriminators.TryAdd(rule.Name, discriminator);
            }
            else
            {
                discriminator = value;
            }

            return discriminator.GetDiscriminator(context, rule);
        }
    }
}
