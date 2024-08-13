using RateLimiter.Examples.Policies;
using RateLimiter.Extensions;
namespace RateLimiter.Examples;

public static class PoliciesRegistartion
{
    public static IServiceCollection RegisterRateLimiterPolicies(this IServiceCollection services)
    {
        // Use default storage provided
        services.UseRateLimiterInMemoryStorage();

        // Add default policies
        services.AddFixedWindowRateLimiterPolicy(RateLimiterPolicyNames.DefaultFixedWindowPolicy);
        services.AddFixedWindowRateLimiterPolicy(RateLimiterPolicyNames.DefaultAllPostFixedWindowPolicy);
        services.AddSlidingWindowRateLimiterPolicy(RateLimiterPolicyNames.DefaultSlidingWindowPolicy);

        // Register custom policies by key
        services.AddKeyedSingleton<IRateLimiterPolicy, MyCustomRequestsSizePolicy>(RateLimiterPolicyNames.MyCustomRequestsSizePolicy);
        services.AddKeyedSingleton<IRateLimiterPolicy, MyCustomComplexGeoPolicy>(RateLimiterPolicyNames.MyCustomComplexGeoPolicy);
        services.AddKeyedTransient<IRateLimiterPolicy, MyCustomSimplePolicy>(RateLimiterPolicyNames.MyCustomSimplePolicy);
        return services;
    }

}

public static class RateLimiterPolicyNames
{
    public const string DefaultAllPostFixedWindowPolicy = nameof(DefaultAllPostFixedWindowPolicy);
    public const string DefaultFixedWindowPolicy = nameof(DefaultFixedWindowPolicy);
    public const string DefaultSlidingWindowPolicy = nameof(DefaultSlidingWindowPolicy);

    public const string MyCustomRequestsSizePolicy = nameof(MyCustomRequestsSizePolicy);
    public const string MyCustomComplexGeoPolicy = nameof(MyCustomComplexGeoPolicy);
    public const string MyCustomSimplePolicy = nameof(MyCustomSimplePolicy);
}