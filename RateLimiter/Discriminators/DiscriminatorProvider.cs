using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;
using System.Collections;
using System.Collections.Generic;

namespace RateLimiter.Discriminators
{
    public class DiscriminatorProvider : IProvideDiscriminatorValues
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

        public Hashtable GetDiscriminatorValues(
            HttpContext context,
            IEnumerable<IDefineARateLimitRule> rules)
        {
            // TODO: These values should likely be cached in the caller

            var results = new Hashtable();

            // for each rule in here, we need to generate the discriminator value
            foreach (var rule in rules)
            {
                switch (rule.Discriminator)
                {
                    case LimiterDiscriminator.QueryString:
                        var qsd = new QueryStringDiscriminator();
                        var qsdResult = qsd.GetDiscriminator(context, rule);
                        results.Add(rule.Name, qsdResult);
                        break;
                    case LimiterDiscriminator.RequestHeader:
                        if (string.IsNullOrEmpty(rule.DiscriminatorRequestHeaderKey))
                        {
                            // TODO: Log
                            throw new MissingFieldException($"{nameof(rule.DiscriminatorRequestHeaderKey)} was not provided");
                        }
                        results.Add(rule.Name, context.Request.Query[rule.DiscriminatorRequestHeaderKey]);
                        break;
                    case LimiterDiscriminator.IpAddress:
                        var ipad = new IpAddressDiscriminator();
                        var ipadResult = ipad.GetDiscriminator(context, rule);
                        results.Add(rule.Name, ipadResult);
                        break;
                    case LimiterDiscriminator.Custom:
                        // hmmm ... need to instantiate the custom discriminator registered and execute it?
                        if (string.IsNullOrEmpty(rule.CustomDiscriminatorName))
                        {
                            throw new MissingFieldException("No value for {@CustomDiscriminatorName",
                                nameof(rule.CustomDiscriminatorName));
                        }

                        var foo = _serviceProvider.GetRequiredKeyedService<IProvideADiscriminator>(rule.CustomDiscriminatorName);
                        var fooValue = foo.GetDiscriminator(context, rule);
                        results.Add(rule.Name, fooValue);
                        break;
                    case LimiterDiscriminator.GeoLocation:
                    case LimiterDiscriminator.IpSubNet:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return results;
        }
    }
}
