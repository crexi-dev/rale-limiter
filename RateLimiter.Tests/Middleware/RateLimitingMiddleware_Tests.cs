using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Moq;
using NUnit.Framework;
using RateLimiter.Api.Middlewares;
using RateLimiter.Rules;
using RateLimiter.Storages;
using RateLimiter.Tests.Shared;
using System;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Middleware;

[TestFixture]
public class RateLimitingMiddleware_Tests
{
    private RateLimitingMiddleware _middleware;
    private Mock<RequestDelegate> _nextDelegateMock;
    private Mock<IRateLimitStore> _storeMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IRateLimitRule> _ruleMock;
    private Endpoint _endpoint;
    private DefaultHttpContext _httpContext;

    [SetUp]
    public void Setup()
    {
        _nextDelegateMock = new Mock<RequestDelegate>();
        _storeMock = new Mock<IRateLimitStore>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _ruleMock = new Mock<IRateLimitRule>();

        _middleware = new RateLimitingMiddleware(_nextDelegateMock.Object, _serviceProviderMock.Object, _storeMock.Object);

        // Create a concrete RouteEndpoint with the necessary metadata
        var routePattern = RoutePatternFactory.Parse("/test");
        _endpoint = new RouteEndpoint(
            context => Task.CompletedTask,
            routePattern,
            order: 0,
            new EndpointMetadataCollection(new RateLimitAttributeMock()),
            "TestEndpoint"
        );

        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Headers["Authorization"] = "client1";
        _httpContext.SetEndpoint(_endpoint);
    }

    [Test]
    public async Task AllowsRequestWhenRuleAllows()
    {
        // Arrange
        _=_ruleMock.Setup(r => r.IsRequestAllowedAsync("client1", "TestEndpoint", _storeMock.Object)).ReturnsAsync(true);
        _=_serviceProviderMock.Setup(s => s.GetService(typeof(IRateLimitRule))).Returns(_ruleMock.Object);
        _=_nextDelegateMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _nextDelegateMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Once);
    }

    [Test]
    public async Task DeniesRequestWhenRuleDenies()
    {
        // Arrange
        _=_ruleMock.Setup(r => r.IsRequestAllowedAsync("client1", "TestEndpoint", _storeMock.Object)).ReturnsAsync(false);
        _=_serviceProviderMock.Setup(s => s.GetService(typeof(IRateLimitRule))).Returns(_ruleMock.Object);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status429TooManyRequests, _httpContext.Response.StatusCode);
        _nextDelegateMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    [Test]
    public async Task SkipsMiddlewareWhenNoEndpoint()
    {
        // Arrange
        _httpContext.SetEndpoint(null);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _nextDelegateMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Once);
    }

    [Test]
    public async Task ReturnsUnauthorizedWhenClientIdMissing()
    {
        // Arrange
        _=_httpContext.Request.Headers.Remove("Authorization");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status401Unauthorized, _httpContext.Response.StatusCode);
        _nextDelegateMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }
}