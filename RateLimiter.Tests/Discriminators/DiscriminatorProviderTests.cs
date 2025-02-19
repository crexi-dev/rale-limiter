using FluentAssertions;

using RateLimiter.Config;
using RateLimiter.Discriminators;

using System.Collections.Generic;

using Xunit;

namespace RateLimiter.Tests.Discriminators
{
    public class DiscriminatorProviderTests : UnitTestBase<DiscriminatorProviderTests>
    {
        [Fact]
        public void GenerateDiscriminators_OnValidData_GeneratesDiscriminators()
        {
            // arrange
            var config = new List<RateLimiterConfiguration.DiscriminatorConfiguration>()
            {

            };

            var sut = Mocker.CreateInstance<DiscriminatorProvider>();

            // act
            var result = sut.GenerateDiscriminators(config);

            // assert
            result.Count.Should().Be(config.Count);
        }
    }
}
