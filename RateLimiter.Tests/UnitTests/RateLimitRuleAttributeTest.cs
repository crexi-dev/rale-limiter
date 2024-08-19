using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Enums;
using RateLimiter.CustomAttributes;
using RateLimiter.Dtos;
using RateLimiter.Interfaces;

[TestFixture]
public class RateLimitAttributeTests
{
    private Mock<ActionExecutionDelegate> _nextMock;
    private Mock<Func<RateLimitRules, IRateLimitRule>> _serviceFactoryMock;
    private ActionExecutingContext _context;
    private RateLimitAttribute _attribute;
    private RateLimitRules _ruleA;
    private RateLimitRules _ruleB;

    [SetUp]
    public void SetUp()
    {
        _nextMock = new Mock<ActionExecutionDelegate>();
        _serviceFactoryMock = new Mock<Func<RateLimitRules, IRateLimitRule>>();
        _context = CreateContext();
        _ruleA = RateLimitRules.RuleA; // Replace with actual enum value
        _ruleB = RateLimitRules.RuleB; // Replace with actual enum value
        _attribute = new RateLimitAttribute(_ruleA, _ruleB);
    }

    [Test]
    public async Task OnActionExecutionAsync_ServiceFactoryIsNull_ReturnsInternalServerError()
    {
        _context.HttpContext.RequestServices = CreateServiceProvider(null);

        await _attribute.OnActionExecutionAsync(_context, _nextMock.Object);

        var result = _context.Result as ContentResult;
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        result.Content.Should().Be("Rate limit service not available.");
    }

    [Test]
    public async Task OnActionExecutionAsync_RequestIsRateLimited_ReturnsTooManyRequests()
    {
        var ruleServiceMock = new Mock<IRateLimitRule>();
        ruleServiceMock.Setup(r => r.IsRequestAllowed(It.IsAny<RateLimitRuleRequestDto>())).ReturnsAsync(false);
        _serviceFactoryMock.Setup(f => f(_ruleA)).Returns(ruleServiceMock.Object);

        _context.HttpContext.RequestServices = CreateServiceProvider(_serviceFactoryMock.Object);

        await _attribute.OnActionExecutionAsync(_context, _nextMock.Object);

        var result = _context.Result as ContentResult;
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        result.Content.Should().Be("Too many requests. Please try again later.");
    }

    [Test]
    public async Task OnActionExecutionAsync_RequestIsNotRateLimited_ProceedsToNext()
    {
        var ruleServiceMock = new Mock<IRateLimitRule>();
        ruleServiceMock.Setup(r => r.IsRequestAllowed(It.IsAny<RateLimitRuleRequestDto>())).ReturnsAsync(true);
        _serviceFactoryMock.Setup(f => f(It.IsAny<RateLimitRules>())).Returns(ruleServiceMock.Object);

        _context.HttpContext.RequestServices = CreateServiceProvider(_serviceFactoryMock.Object);

        await _attribute.OnActionExecutionAsync(_context, _nextMock.Object);

        _nextMock.Verify(n => n(), Times.Once);
        _context.Result.Should().BeNull();
    }

    private ActionExecutingContext CreateContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Token"] = "dummy-token";
        return new ActionExecutingContext(
            new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new object()
        );
    }

    private IServiceProvider CreateServiceProvider(Func<RateLimitRules, IRateLimitRule> serviceFactory)
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(Func<RateLimitRules, IRateLimitRule>)))
                           .Returns(serviceFactory);
        return serviceProviderMock.Object;
    }
}
