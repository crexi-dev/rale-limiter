using Microsoft.AspNetCore.Http;

using RateLimiter.Abstractions;

using System;

namespace RateLimiter.Discriminators
{
    public class IpAddressDiscriminator : IProvideADiscriminator
    {
        public string GetDiscriminator(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
