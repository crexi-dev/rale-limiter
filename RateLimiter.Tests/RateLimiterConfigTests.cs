using FluentAssertions;

namespace RateLimiter.Tests;

public class RateLimiterConfigTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void RateLimiterConfig_CacheExpirationInMinutes_ZeroOrNegative_ValidationFails(int cacheExpirationInMinutes)
    {
        // arrange
        var config = new RateLimiterConfig()
        {
            CacheExpirationInMinutes = cacheExpirationInMinutes
        };
        var validator = new RateLimiterConfigValidator();

        // act
        var result = validator.Validate(config);

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be($"'Cache Expiration In Minutes' must be greater than '0'.");
    }
}