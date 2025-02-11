using FluentAssertions;

using RateLimiter.Enums;
using RateLimiter.Rules.Algorithms;

using System;

using Xunit;

namespace RateLimiter.Tests.Rules.Algorithms
{
    public class AlgorithmProviderTests : UnitTestBase<AlgorithmProviderTests>
    {
        [Theory]
        [InlineData(RateLimitingAlgorithm.Default, RateLimitingAlgorithm.FixedWindow)]
        [InlineData(RateLimitingAlgorithm.FixedWindow, RateLimitingAlgorithm.FixedWindow)]
        [InlineData(RateLimitingAlgorithm.LeakyBucket, RateLimitingAlgorithm.LeakyBucket)]
        [InlineData(RateLimitingAlgorithm.SlidingWindow, RateLimitingAlgorithm.SlidingWindow)]
        [InlineData(RateLimitingAlgorithm.TokenBucket, RateLimitingAlgorithm.TokenBucket)]
        public void GetAlgorithm_WithValidData_ProvidesCorrectAlgorithm(
            RateLimitingAlgorithm algo,
            RateLimitingAlgorithm expectedAlgorithm)
        {
            // arrange
            var sut = Mocker.CreateInstance<AlgorithmProvider>();

            // act
            var result = sut.GetAlgorithm(algo, 5, TimeSpan.FromMilliseconds(3000));

            // assert
            //result.Name.Should().Be(typeof(algo));
            result.Algorithm.Should().Be(expectedAlgorithm);
        }
    }
}
