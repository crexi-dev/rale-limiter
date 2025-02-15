
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
    /// <summary>
    /// Note: This test class is not true unit testing and would not exist in this project
    /// Chose to use concrete implementations in some places to facilitate functional testing
    /// </summary>
	[Fact]
    public void IsRequestAllowed()
    {
        var mocker = new AutoMocker();
        var fixture = new Fixture();

        // arrange
        var appOptions = Options.Create<RateLimiterConfiguration>(new RateLimiterConfiguration()
        {
            Algorithms =
            [
                new AlgorithmConfiguration()
                {
                    Name = "RequestsPerTimeSpan0",
                    Parameters = new AlgorithmConfiguration.AlgorithmConfigurationParameters()
                    {
                        MaxRequests = 3,
                        WindowDurationMS = 3000
                    },
                    Type = AlgorithmType.FixedWindow
                },
                new AlgorithmConfiguration()
                {
                    Name = "TimeSpanElapsed0",
                    Parameters = new AlgorithmConfiguration.AlgorithmConfigurationParameters()
                    {
                        MinIntervalMS = 3000
                    },
                    Type = AlgorithmType.TimespanElapsed
                }
            ],
            Rules =
            [
                new RuleConfiguration()
                {
                    Name = "IpAddressRule",
                    Discriminators = ["IpAddressDisc"]
                }
            ],
            Discriminators =
            [
                new DiscriminatorConfiguration()
                {
                    Name = "IpAddressDisc",
                    Type = DiscriminatorType.IpAddress,
                    AlgorithmNames = ["RequestsPerTimeSpan0"]
                }
            ]
        });

        mocker.GetMock<IOptions<RateLimiterConfiguration>>()
            .Setup(s => s.Value)
            .Returns(appOptions.Value);

        mocker.Use<IDateTimeProvider>(new DateTimeProvider());
        mocker.Use<IRateLimitDiscriminatorProvider>(new DiscriminatorProvider(null, null));

        // mock the rule attribute as would be applied to our resource's endpoint
        var rateLimitedResources = new List<RateLimitedResource>()
        {
            fixture.Build<RateLimitedResource>()
                .With(x => x.RuleName, "IpAddressRule")
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
        mocker.Use<IRateLimitAlgorithmProvider>(algoProvider);

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
}