//using FluentAssertions;

//using RateLimiter.Enums;
//using RateLimiter.Rules.Algorithms;

//using System;

//using Xunit;

//namespace RateLimiter.Tests.Rules.Algorithms
//{
//    public class AlgorithmProviderTests : UnitTestBase<AlgorithmProviderTests>
//    {
//        [Theory]
//        [InlineData(AlgorithmType.Default, AlgorithmType.FixedWindow)]
//        [InlineData(AlgorithmType.FixedWindow, AlgorithmType.FixedWindow)]
//        [InlineData(AlgorithmType.LeakyBucket, AlgorithmType.LeakyBucket)]
//        [InlineData(AlgorithmType.SlidingWindow, AlgorithmType.SlidingWindow)]
//        [InlineData(AlgorithmType.TokenBucket, AlgorithmType.TokenBucket)]
//        public void GetAlgorithm_WithValidData_ProvidesCorrectAlgorithm(
//            AlgorithmType algo,
//            AlgorithmType expectedAlgorithmType)
//        {
//            // arrange
//            var sut = Mocker.CreateInstance<AlgorithmProvider>();

//            // act
//            var result = sut.GetAlgorithm(algo, 5, TimeSpan.FromMilliseconds(3000));

//            // assert
//            result.AlgorithmType.Should().Be(expectedAlgorithmType);
//        }
//    }
//}
