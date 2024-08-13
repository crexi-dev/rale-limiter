using Microsoft.AspNetCore.Http;
using RateLimiter.Configuration;
using System.Collections.Generic;

namespace RateLimiter.Policies;

/// <summary>
/// Optional base class for RateLimiter policies 
/// </summary>
public abstract class RateLimiterPolicy : IRateLimiterPolicy
{
    /// <summary>
    /// Provides GroupId for HTTP request based on default policy settings template
    /// GroupId is used to identify the group of request this policy is applied to
    /// </summary>
    protected static string GetGroupId(HttpRequest request, BasePolicySettings settings, string policyName)
    {
        var groupId = policyName;
        if (settings.IsEndpointSpecific)
        {
            groupId += $"_{request.Path.GetHashCode()}";
        }
        if (settings.IsClientSpecific)
        {
            if (request.Headers.TryGetValue(settings.ClientIdHeaderName, out var clientId))
            {
                groupId += $"_{clientId}";
            }
        }
        return groupId;
    }

    public abstract IEnumerable<IRateLimiterRule> GetRuleset(HttpRequest request);
}
