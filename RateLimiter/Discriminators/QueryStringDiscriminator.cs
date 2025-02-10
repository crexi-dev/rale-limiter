using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

using System;

namespace RateLimiter.Discriminators
{
    public class QueryStringDiscriminator : IProvideADiscriminator
    {
        public string GetDiscriminator(HttpContext context, IDefineARateLimitRule rateLimitRule)
        {
            throw new NotImplementedException();
        }
    }
}
