using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace RateLimiter.Discriminators
{
    public class DiscriminatorProvider : IProvideDiscriminators
    {
        private readonly IServiceProvider _serviceProvider;

        public DiscriminatorProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Hashtable GetDiscriminators(
            HttpContext context,
            IEnumerable<IDefineRateLimitRules> rules)
        {
            // TODO: These values should likely be cached in the caller

            var results = new Hashtable();

            // for each rule in here, we need to generate the discriminator value
            foreach (var rule in rules)
            {
                // TODO: Create discriminator-specific classes for each of these
                switch (rule.Discriminator)
                {
                    case LimiterDiscriminator.ApiKey:
                        results.Add(rule.Name, context.Request.Query["api-key"]);
                        break;
                    case LimiterDiscriminator.RequestHeader:
                        if (string.IsNullOrEmpty(rule.DiscriminatorRequestHeaderKey))
                        {
                            // log
                            throw new MissingFieldException(
                                $"{nameof(rule.DiscriminatorRequestHeaderKey)} was not provided");
                        }
                        results.Add(rule.Name, context.Request.Query[rule.DiscriminatorRequestHeaderKey]);
                        break;
                    case LimiterDiscriminator.IpAddress:
                        // TODO: This is likely incorrect. Cannot test b/c shows "localhost"
                        results.Add(rule.Name, context.Request.Headers.Host);
                        break;
                    case LimiterDiscriminator.Custom:
                        // hmmm ... need to instantiate the custom discriminator registered and execute it?
                        if (string.IsNullOrEmpty(rule.CustomDiscriminatorName))
                        {
                            throw new MissingFieldException("No value for {@CustomDiscriminatorName",
                                nameof(rule.CustomDiscriminatorName));
                        }

                        var foo = _serviceProvider.GetRequiredKeyedService<IProvideADiscriminator>(rule.CustomDiscriminatorName);
                        var fooValue = foo.GetDiscriminator(context);
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
