using Microsoft.Extensions.Configuration;
using System;

namespace RateLimiter.Extensions;

public static class ConfigurationExtensions
{
    public const string RateLimiterPoliciesSectionName = "RateLimiterPolicies";

    /// <summary>
    /// Returns policy settings from default RateLimiter Policies section in configuration by policy name
    /// </summary>
    /// <exception cref="ArgumentException">Confuration is missing or in incorrect format</exception>
    public static TSettings GetRateLimiterPolicySettings<TSettings>(this IConfiguration configuration, string policyName) =>
        configuration.GetSection(RateLimiterPoliciesSectionName).GetSection(policyName).Get<TSettings>()
            ?? throw new ArgumentException("Confuration is missing or in incorrect format");
}
