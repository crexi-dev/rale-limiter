using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RateLimiter.Abstractions;

public interface IProvideDiscriminators
{
    Hashtable GetDiscriminators(
        HttpContext context,
        IEnumerable<IDefineRateLimitRules> rules);
}