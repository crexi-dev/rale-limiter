using System;

namespace RateLimiter.Tests;

public static class SampleRules
{
    // A global rule. All requests having this scope will be checked.
    public static Rule GlobalRule = new Rule(
        "payment-api",
        5,
        TimeSpan.FromSeconds(1));

    // An API rule specific to a route.
    public static Rule ApiRouteRule = new Rule(
        "payment-api-credit-check",
        10,
        TimeSpan.FromMinutes(1))
    {
        AppliesWhen = (req) =>
        {
            var containsPath = false;
            var containsMethod = false;

            if (req.Parameters.TryGetValue<string>("path", out var path))
            {
                containsPath = path.Contains("credit-check", StringComparison.InvariantCultureIgnoreCase);
            }

            if (req.Parameters.TryGetValue<string>("httpMethod", out var method))
            {
                containsMethod = string.Compare("post", method, StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            return containsPath && containsMethod;
        }
    };

    // A rule based on context data. Here, all requests with a location of "us" will be checked.
    public static Rule UnitedStatesRule = new Rule(
        "payment-api-us",
        10,
        TimeSpan.FromSeconds(1))
    {
        AppliesWhen = (req) =>
        {
            if (req.Parameters.TryGetValue<string>("location", out var location))
            {
                return string.Compare(location, "us", StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            return false;
        }
    };

    // A rule based on context data. Here, all requests with a location of "eu" will be checked.
    public static Rule EuropeRule = new Rule(
        "payment-api-eu",
        1,
        TimeSpan.FromSeconds(5))
    {
        AppliesWhen = (req) =>
        {
            if (req.Parameters.TryGetValue<string>("location", out var location))
            {
                return string.Compare(location, "eu", StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            return false;
        }
    };

    // A composite rule that will check for both us and eu based requests, with GlobalRule as a fallback.
    // It's assumed that only one rule will actually be executed. 
    public static CompositeRule Composite = new(new[] { UnitedStatesRule, EuropeRule, GlobalRule });

}

