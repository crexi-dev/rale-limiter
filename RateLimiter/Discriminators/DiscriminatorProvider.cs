using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;
using RateLimiter.Enums;

using System;
using System.Collections;
using System.Collections.Generic;

namespace RateLimiter.Discriminators
{
    public class DiscriminatorProvider : IProvideDiscriminators
    {
        public Hashtable GetDiscriminators(
            HttpContext context,
            IEnumerable<IDefineRateLimitRules> rules)
        {
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
