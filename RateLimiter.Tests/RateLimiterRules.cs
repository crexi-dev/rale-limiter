using System;

namespace RateLimiter.Tests;
public static class RateLimiterRules
{
    public static RateLimitRule TeamBApiRuleUsa = new(
        "team-b",
        new[] { new RateLimitDescriptor("location", "us") },
        new RateLimit(5, TimeSpan.FromMinutes(1)));

    public static RateLimitRule TeamBApiRuleEurope = new(
        "team-b",
        new[] { new RateLimitDescriptor("location", "eu") },
        new RateLimit(1, TimeSpan.FromSeconds(10)));

    public static RateLimitRule MarketingPhoneNumberRule = new(
        "marketing",
        new[] { new RateLimitDescriptor("phone", "555-1234"), new RateLimitDescriptor("phone", "555-5678") },
        new RateLimit(10, TimeSpan.FromHours(1)));

    public static RateLimitRule DatabaseRule = new(
        "database",
        new[] { new RateLimitDescriptor("type", "cosmos") },
        new RateLimit(5, TimeSpan.FromSeconds(1)));

    public static RateLimitRule ExpensiveApiRule = new(
        "research",
        new[] { new EmptyRateLimitDescriptor() },
        new RateLimit(1, TimeSpan.FromSeconds(5)));

    public static RateLimitRule BlacklistRule = new(
        "payment",
        new[]
        {
            new RateLimitDescriptor("userid", "foobar1@gmail.com"),
            new RateLimitDescriptor("userid", "foobar2@gmail.com")
        },
        new RateLimit(0, TimeSpan.Zero));

}
