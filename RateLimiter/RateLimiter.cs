using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public class RateLimiter
{
    /// <summary>
    /// Validates request through set of provided rules
    /// </summary>
    public static async Task<bool> IsRequestAllowedAsync(HttpContext httpContext, IEnumerable<IRateLimiterRule> rules, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var validationTasks = rules.Select(x => x.IsRequestAllowedAsync(httpContext.Request, cts.Token)).ToList();

        while (validationTasks.Any())
        {
            var completedTask = await Task.WhenAny(validationTasks);

            bool result = await completedTask;
            if (!result)
            {
                cts.Cancel();
                return false;
            }

            validationTasks.Remove(completedTask);
        }
        return true;
    }

    /// <summary>
    /// Validates request through set of rules defined by policy
    /// </summary>
    public static async Task<bool> IsRequestAllowedAsync(HttpContext httpContext, IRateLimiterPolicy policy, CancellationToken ct = default) =>
        await IsRequestAllowedAsync(httpContext, policy.GetRuleset(httpContext.Request), ct);

    /// <summary>
    /// Validates request through set of rules defined by policy registered with "policyName" key
    /// </summary>
    public static async Task<bool> IsRequestAllowedAsync(HttpContext httpContext, string policyName, CancellationToken ct = default) =>
        await IsRequestAllowedAsync(httpContext, httpContext.RequestServices.GetRequiredKeyedService<IRateLimiterPolicy>(policyName), ct);
}
