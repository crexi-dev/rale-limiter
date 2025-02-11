using Microsoft.AspNetCore.Http;

using System.Collections.Generic;

namespace RateLimiter.Abstractions;

public interface IProvideDiscriminatorValues
{
    Dictionary<string, (bool IsMatch, string MatchValue)> GetDiscriminatorValues(
        HttpContext context,
        IEnumerable<IDefineARateLimitRule> rules);
}