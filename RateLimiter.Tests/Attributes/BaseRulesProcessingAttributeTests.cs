using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Attributes;
using RateLimiter.Contracts;
using RateLimiter.Infrastructure;
using RateLimiter.Processors;
using RateLimiter.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RateLimiter.Tests.Attributes;

public class BaseRulesProcessingAttributeTests
{
    private readonly BaseRulesProcessingAttribute<AllowRequestResult, RequestDetails> _mockBaseRulesProcessingAttribute;
    private readonly Mock<ILogger<DefaultEngine>> _mockEngineFactoryLogger;
    private readonly Mock<MockProcessorFactory> _mockProcessorFactory;
    private readonly Mock<MockContextExtender> _mockContextExtender;

    public BaseRulesProcessingAttributeTests()
    {
        Mock<ILogger<CachedRequestsRepository>> mockCachedRequestsLogger = new();
        var mockCachedRequestsRepository = new CachedRequestsRepository(mockCachedRequestsLogger.Object);

        Mock<ILogger<BlockedRequestsRepository>> mockBlockedRequestsLogger = new();
        var mockBlockedRequestsRepository = new BlockedRequestsRepository(mockBlockedRequestsLogger.Object);
        Mock<ILogger<BaseRulesProcessingAttribute<AllowRequestResult, RequestDetails>>> mockLogger = new();

        Mock<ILogger<ProcessorFactory>> mockProcessorFactoryLogger = new();
        _mockEngineFactoryLogger = new Mock<ILogger<DefaultEngine>>();
        Mock<EngineFactory> mockEngineFactory = new(_mockEngineFactoryLogger.Object);
        _mockProcessorFactory =
            new Mock<MockProcessorFactory>(mockProcessorFactoryLogger.Object, mockEngineFactory.Object);
        _mockContextExtender = new Mock<MockContextExtender>();

        _mockBaseRulesProcessingAttribute = new BaseRulesProcessingAttribute<AllowRequestResult, RequestDetails>(mockBlockedRequestsRepository,
            mockCachedRequestsRepository,
            mockLogger.Object, _mockProcessorFactory.Object, (IContextExtender)_mockContextExtender.Object);
    }

    [Fact]
    public async Task OnResourceExecutionAsync_RequestBlocked_ReturnsBadRequest()
    {
        // Arrange
        IList<IFilterMetadata> mockaData = new List<IFilterMetadata>();

        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor();

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = routeData,
            ActionDescriptor = actionDescriptor
        };

        var mockContext = new Mock<MockedContext>(actionContext, mockaData, new List<IValueProviderFactory>());
        var mockNext = new Mock<ResourceExecutionDelegate>();

        IEnumerable<IRateRule> testRules = new[] { new FalseRule("testRule") };
        SetupProcessorDetails(testRules);

        // Act
        var theContext = mockContext.Object;
        await _mockBaseRulesProcessingAttribute.OnResourceExecutionAsync(theContext, mockNext.Object);

        // Assert
        var resultContext = theContext as ResourceExecutingContext;
        var result = resultContext?.Result as ContentResult;
        Assert.NotNull(result);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task OnResourceExecutionAsync_RequestNotBlocked_ProcessesRequest()
    {
        // Arrange
        IList<IFilterMetadata> mockaData = new List<IFilterMetadata>();
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor();

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = routeData,
            ActionDescriptor = actionDescriptor
        };

        var mockContext = new Mock<MockedContext>(actionContext, mockaData, new List<IValueProviderFactory>());

        IEnumerable<IRateRule> testRules = new[] { new TrueRule("testRule") };
        SetupProcessorDetails(testRules);

        var resourceExecutionDelegate = ResourceExecutionDelegateMock.Create();

        // Act
        await _mockBaseRulesProcessingAttribute.OnResourceExecutionAsync(mockContext.Object,
            resourceExecutionDelegate.Object);

        // Assert
        resourceExecutionDelegate.Verify(x => x(), Times.Once);
    }

    private void SetupProcessorDetails(IEnumerable<IRateRule> testRules)
    {
        var requestDetails = new RequestDetails(Guid.NewGuid().ToString(), "US", "api/widget", DateTime.Now);
        _mockContextExtender.Setup(x => x.CreateRequestDetailsFromContext(It.IsAny<HttpRequest>()))
            .Returns(Task.Run(() => requestDetails));

        IEnumerable<RequestDetails> testContext = new[]
            { new RequestDetails(Guid.NewGuid().ToString(), "US", "api/widget", DateTime.Now) };

        _mockProcessorFactory.Setup(x =>
                x.GetContextualProcessor<AllowRequestResult, RequestDetails>(It.IsAny<IEnumerable<IRateRule>>(),
                    It.IsAny<IEnumerable<RequestDetails>>()))
            .Returns(Task.Run(() =>
            {
                var rulesProcessor = new RateRulesProcessor(testRules,
                    new DefaultEngine(_mockEngineFactoryLogger.Object), testContext);

                return rulesProcessor as IContextualRulesProcessor<IRateRule, AllowRequestResult, RequestDetails>;
            })!);
    }
}

public class MockContextExtender : IContextExtender
{
    public virtual async Task<RequestDetails> CreateRequestDetailsFromContext(HttpRequest request)
    {
        return await Task.Run(() => new RequestDetails());
    }
}

public class MockedContext : ResourceExecutingContext
{
    public MockedContext(ActionContext actionContext,
        IList<IFilterMetadata> filters,
        IList<IValueProviderFactory> valueProviderFactories) : base(actionContext, filters, valueProviderFactories)
    {
    }

    public IActionResult Result { get; set; }
}

public class ResourceExecutionDelegateMock
{
    public static Mock<ResourceExecutionDelegate> Create()
    {
        var mockDelegate = new Mock<ResourceExecutionDelegate>();

        mockDelegate.Setup(d => d())
            .ReturnsAsync(new ResourceExecutedContext(
                new ActionContext(new DefaultHttpContext(), new Microsoft.AspNetCore.Routing.RouteData(),
                    new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()),
                new List<IFilterMetadata>())
            );

        return mockDelegate;
    }
}

public class MockProcessorFactory : IProcessorFactory
{
    private readonly ILogger<ProcessorFactory> _logger;
    private readonly EngineFactory _engineFactory;

    public MockProcessorFactory(ILogger<ProcessorFactory> logger, EngineFactory engineFactory)
    {
        _logger = logger;
        _engineFactory = engineFactory;
    }

    public virtual async Task<IContextualRulesProcessor<IRateRule, TReturnType, TContextType>?>
        GetContextualProcessor<TReturnType, TContextType>(IEnumerable<IRateRule> rules,
            IEnumerable<RequestDetails>? rulesContext)
    {
        return null;
    }

    public virtual List<IRateRule> GetDefaultRuleset(string location)
    {
        return [];
    }
}