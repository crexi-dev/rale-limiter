
using AutoFixture;

using FluentAssertions;

using HttpContextMoq;
using HttpContextMoq.Extensions;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Moq.AutoMock;

using RateLimiter.Abstractions;
using RateLimiter.Common;
using RateLimiter.Config;
using RateLimiter.Discriminators;
using RateLimiter.Enums;
using RateLimiter.Rules.Algorithms;

using System.Collections.Generic;
using System.Threading;

using Xunit;

using static RateLimiter.Config.RateLimiterConfiguration;

namespace RateLimiter.Tests;

public class RateLimiterTest
{
	[Fact]
	public void IsRequestAllowed()
    {
        var mocker = new AutoMocker();
        var fixture = new Fixture();

        // arrange
        var appOptions = Options.Create<RateLimiterConfiguration>(new RateLimiterConfiguration()
        {
            DefaultAlgorithm = RateLimitingAlgorithm.FixedWindow,
            DefaultMaxRequests = 5,
            DefaultTimespanMilliseconds = 3000,
            Rules = GenerateRateLimitRules()
        });

        mocker.GetMock<IOptions<RateLimiterConfiguration>>()
            .Setup(s => s.Value)
            .Returns(appOptions.Value);

        mocker.Use<IDateTimeProvider>(new DateTimeProvider());
        mocker.Use<IProvideDiscriminatorValues>(new DiscriminatorProvider(null, null));

        //// mock the rules as would be defined within appSettings
        //var rateLimitRules = GenerateRateLimitRules();
        //mocker.GetMock<IProvideRateLimitRules>()
        //    .Setup(s => s.GetRules(new RateLimiterConfiguration()))
        //    .Returns(rateLimitRules);
        mocker.Use<IProvideRateLimitRules>(new RateLimiterRulesFactory());

        // mock the rule attribute as would be applied to our resource's endpoint
        var rateLimitedResources = new List<RateLimitedResource>()
        {
            fixture.Build<RateLimitedResource>()
                .With(x => x.RuleName, "RequestPerTimespan-Default")
                .Create()
        };

        var context = new HttpContextMock()
            .SetupUrl("http://localhost:8000/path")
            .SetupRequestHeaders(new Dictionary<string, StringValues>()
            {
                { "Host", "192.168.0.1"}
            })
            .SetupRequestMethod("GET");

        var algoProvider = mocker.CreateInstance<AlgorithmProvider>();
        mocker.Use<IProvideRateLimitAlgorithms>(algoProvider);

        var limiter = mocker.CreateInstance<RateLimiter>();
        
        // act
        const int numberOfRequestsToTry = 4;

        for (var i = 0; i < numberOfRequestsToTry; i++)
        {
            var result = limiter.IsRequestAllowed(context, rateLimitedResources);
            
            // assert
            if (i <= 2)
            {
                result.RequestIsAllowed.Should().BeTrue();
                result.ErrorMessage.Should().BeNullOrEmpty();
            }
            else
            {
                result.RequestIsAllowed.Should().BeFalse();
                result.ErrorMessage.Should().NotBeNullOrEmpty();

                // wait 3 seconds
                Thread.Sleep(3000);

                result = limiter.IsRequestAllowed(context, rateLimitedResources);

                result.RequestIsAllowed.Should().BeTrue();
                result.ErrorMessage.Should().BeNullOrEmpty();
            }
        }
    }

    private static List<RateLimiterRuleItemConfiguration> GenerateRateLimitRules()
    {
        var fixture = new Fixture();
        var values = new List<RateLimiterRuleItemConfiguration>
        {
            fixture.Build<RateLimiterRuleItemConfiguration>()
                .With(x => x.Name, "RequestPerTimespan-Default")
                .With(x => x.Type, LimiterType.RequestsPerTimespan)
                .With(x => x.Discriminator, LimiterDiscriminator.IpAddress)
                .With(x => x.DiscriminatorMatch, string.Empty)
                .With(x => x.DiscriminatorRequestHeaderKey, string.Empty)
                .With(x => x.MaxRequests, 3)
                .With(x => x.TimespanMilliseconds, 3000)
                .With(x => x.Algorithm, RateLimitingAlgorithm.Default)
                .Create(),
            fixture.Build<RateLimiterRuleItemConfiguration>()
                .With(x => x.Name, "ApiKey-Default")
                .With(x => x.Type, LimiterType.RequestsPerTimespan)
                .With(x => x.Discriminator, LimiterDiscriminator.QueryString)
                .With(x => x.DiscriminatorMatch, "x-crexi-token")
                .With(x => x.DiscriminatorRequestHeaderKey, "US")
                .With(x => x.Algorithm, RateLimitingAlgorithm.Default)
                .With(x => x.TimespanMilliseconds, 4000)
                .Create()
        };

        return values;
    }
}