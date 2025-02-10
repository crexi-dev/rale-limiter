using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RateLimiter.Abstractions;

public interface IProvideDiscriminatorValues
{
    Hashtable GetDiscriminatorValues(
        HttpContext context,
        IEnumerable<IDefineARateLimitRule> rules);
}