using FluentAssertions;

using HttpContextMoq;
using HttpContextMoq.Extensions;

using Microsoft.Extensions.Primitives;

using RateLimiter.Abstractions;
using RateLimiter.Discriminators;
using RateLimiter.Enums;
using RateLimiter.Rules;

using System;
using System.Collections.Generic;

using Xunit;

namespace RateLimiter.Tests.Discriminators
{
    public class DiscriminatorProviderTests : UnitTestBase<DiscriminatorProviderTests>
    {
        [Fact]
        public void GetDiscriminatorValues_OnValidData_GetsValues()
        {
            // arrange
            var context = new HttpContextMock()
                .SetupUrl("http://localhost:8000/path")
                .SetupRequestHeaders(new Dictionary<string, StringValues>()
                {
                    { "Host", "192.168.0.1"}
                })
                .SetupRequestMethod("GET");

            var sut = Mocker.CreateInstance<DiscriminatorProvider>();

            // act
            var result = sut.GetDiscriminatorValues(context, rules);

            // assert
            result.Count.Should().Be(rules.Count);
        }

        private List<IDefineARateLimitRule> rules =
        [
            new RequestPerTimespanRule()
            {
                Algorithm = RateLimitingAlgorithm.FixedWindow,
                CustomDiscriminatorName = string.Empty,
                Discriminator = LimiterDiscriminator.QueryString,
                DiscriminatorMatch = "someQuerystringValue",
                DiscriminatorKey = string.Empty,
                MaxRequests = 5,
                Name = $"My{nameof(LimiterDiscriminator.QueryString)}",
                TimespanMilliseconds = TimeSpan.FromMilliseconds(1000)
            },

            new RequestPerTimespanRule()
            {
                Algorithm = RateLimitingAlgorithm.FixedWindow,
                CustomDiscriminatorName = string.Empty,
                Discriminator = LimiterDiscriminator.RequestHeader,
                DiscriminatorMatch = string.Empty,
                DiscriminatorKey = "Host",
                MaxRequests = 5,
                Name = $"My{nameof(LimiterDiscriminator.RequestHeader)}",
                TimespanMilliseconds = TimeSpan.FromMilliseconds(1000)
            },

            new RequestPerTimespanRule()
            {
                Algorithm = RateLimitingAlgorithm.FixedWindow,
                CustomDiscriminatorName = string.Empty,
                Discriminator = LimiterDiscriminator.IpAddress,
                DiscriminatorMatch = string.Empty,
                DiscriminatorKey = string.Empty,
                MaxRequests = 5,
                Name = $"My{nameof(LimiterDiscriminator.IpAddress)}",
                TimespanMilliseconds = TimeSpan.FromMilliseconds(1000)
            }
        ];
    }
}
