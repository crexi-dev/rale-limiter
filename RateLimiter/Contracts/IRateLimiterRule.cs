using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public interface IRateLimiterRule
{
    /// <summary>
    /// Validates particular request
    /// </summary>
    /// <returns>true if request is allowed by this rule</returns>
    Task<bool> IsRequestAllowedAsync(HttpRequest request, CancellationToken ct = default);
}
