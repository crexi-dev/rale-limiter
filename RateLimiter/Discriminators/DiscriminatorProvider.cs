using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RateLimiter.Abstractions;
using RateLimiter.Config;
using RateLimiter.Enums;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Discriminators
{
    public class DiscriminatorProvider : IRateLimitDiscriminatorProvider
    {
        private readonly ILogger<DiscriminatorProvider> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DiscriminatorProvider(
            ILogger<DiscriminatorProvider> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public ConcurrentDictionary<string, IRateLimitDiscriminator> GenerateDiscriminators(
            List<RateLimiterConfiguration.DiscriminatorConfiguration> configDiscriminators)
        {
            var values = new ConcurrentDictionary<string, IRateLimitDiscriminator>();

            foreach (var disc in configDiscriminators)
            {
                switch (disc.Type)
                {
                    case DiscriminatorType.Custom:
                        var customDiscriminator = _serviceProvider
                            .GetRequiredKeyedService<IRateLimitDiscriminator>(disc.CustomDiscriminatorType);
                        customDiscriminator.Configuration = disc;
                        values.TryAdd(disc.Name, customDiscriminator);
                        break;
                    case DiscriminatorType.GeoLocation:
                        values.TryAdd(disc.Name, new GeoBasedDiscriminator(disc));
                        break;
                    case DiscriminatorType.IpAddress:
                        values.TryAdd(disc.Name, new IpAddressDiscriminator(disc));
                        break;
                    case DiscriminatorType.QueryString:
                        values.TryAdd(disc.Name, new QueryStringDiscriminator(disc));
                        break;
                    case DiscriminatorType.RequestHeader:
                        values.TryAdd(disc.Name, new RequestHeaderDiscriminator(disc));
                        break;
                    case DiscriminatorType.IpSubNet:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return values;
        }
    }
}
