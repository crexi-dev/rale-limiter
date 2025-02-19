using AutoFixture;

using FluentAssertions;

using RateLimiter.Config;
using RateLimiter.Enums;

using Xunit;

namespace RateLimiter.Tests.Config
{
    public class RateLimiterConfigurationValidatorTests : UnitTestBase<RateLimiterConfigurationValidator>
    {
        [Fact]
        public void Validate_WhenMissingAlgorithmConfigurationVars_FailsValidation()
        {
            // arrange
            var config = Fixture.Build<RateLimiterConfiguration>()
                .With(x => x.Algorithms, [
                    Fixture.Build<RateLimiterConfiguration.AlgorithmConfiguration>()
                        .With(x => x.Type, AlgorithmType.FixedWindow)
                        .With(x => x.Parameters,
                            new RateLimiterConfiguration.AlgorithmConfiguration.AlgorithmConfigurationParameters()
                            {
                                MaxRequests = 5,
                                WindowDurationMS = null
                            })
                        .OmitAutoProperties()
                        .Create()
                ])
                .OmitAutoProperties()
                .Create();

            var validator = Mocker.CreateInstance<RateLimiterConfigurationValidator>();

            // act
            var result = validator.Validate(config);

            // assert
            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
        }
    }
}
