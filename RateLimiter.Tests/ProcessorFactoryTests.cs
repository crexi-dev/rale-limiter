using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using RateLimiter.Infrastructure;
using RateLimiter.Processors;
using RateLimiter.Rules;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ProcessorFactoryTests
{
    private readonly Mock<ILogger<ProcessorFactory>> _loggerMock;
    private readonly EngineFactory _engineFactoryMock;
    private readonly ProcessorFactory _processorFactory;

    public ProcessorFactoryTests()
    {
        _loggerMock = new Mock<ILogger<ProcessorFactory>>();
        var _engineLoggerMock = new Mock<ILogger<DefaultEngine>>();
        _engineFactoryMock = new EngineFactory(_engineLoggerMock.Object);
        _processorFactory = new ProcessorFactory(_loggerMock.Object, _engineFactoryMock);
    }

    [Fact]
    public async Task GetContextualProcessor_ShouldReturnProcessor_WhenCalled()
    {
        // Arrange
        var rules = new List<IRateRule> { new SampleRateRule() };
        var rulesContext = new List<RequestDetails> { new SampleRequestDetails() };
        var mockEngine = new Mock<DefaultEngine>();

        // Act
        var result = await _processorFactory.GetContextualProcessor<AllowRequestResult, RequestDetails>(rules.AsEnumerable(), rulesContext);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IContextualRulesProcessor<IRateRule, AllowRequestResult, RequestDetails>>(result);
    }

    [Theory]
    [InlineData("US", typeof(UsRuleSet))]
    [InlineData("EUR", typeof(EuropeRuleset))]
    [InlineData("RES", typeof(RestrictedRegionRuleset))]
    [InlineData("PAC", typeof(PacificRegionRuleset))]
    [InlineData("Other", typeof(DefaultRuleset))]
    public void GetDefaultRuleset_ShouldReturnExpectedRuleset_WhenLocationIsProvided(string location, Type expectedType)
    {
        // Act
        var result = _processorFactory.GetDefaultRuleset(location);

        // Assert
        Assert.NotNull(result);
        Assert.IsType(expectedType, result);
    }
}

public class SampleRateRule : IRateRule
{
    public Task<AllowRequestResult> Evaluate(IEnumerable<RequestDetails> context)
    {
        return Task.FromResult(new AllowRequestResult(true, "sample raterule"));
    }

    public Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>>? Override { get; set; }
}

public class SampleRequestDetails : RequestDetails
{
    public SampleRequestDetails():base(Guid.NewGuid().ToString(), "US", "api/Widget", DateTime.UtcNow)
    { }
}