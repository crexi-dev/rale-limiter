using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using RateLimiter.Api.ApiFilters;
using RateLimiter.Api.Attributes;
using RateLimiter.Contracts;
using RateLimiter.Enums;
using RateLimiter.Models;
using Xunit;

namespace RateLimiter.Tests;

public class RateLimitFilterTests
{
    private readonly Mock<IRateLimitExecutor> _requestLimitExecutorMock;
    private readonly RateLimitFilter _filter;

    public RateLimitFilterTests()
    {
        _requestLimitExecutorMock = new Mock<IRateLimitExecutor>();
        _filter = new RateLimitFilter(_requestLimitExecutorMock.Object);
    }
    
    [Fact]
    public async Task WithInvalidHeaders_ShouldReturn400StatusCode()
    {
        // Arrange
        var context = CreateActionContext(
            new Dictionary<string, string>
            {
                { "Identifier", "invalid-identifier" },
                { "RegionType", "invalid-region-type" }
            },
            new RateLimitRuleAttribute(RuleType.TimeSpanSinceLastCall, RuleType.RequestPerTimeSpan)
        );
        
        var nextCalled = false;
        var next = CreateNextDelegate(context, () => nextCalled = true);

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled.Should().BeFalse();
        context.Result.Should().BeOfType<ContentResult>().Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(RuleType.TimeSpanSinceLastCall, RuleType.RequestPerTimeSpan)]
    public async Task ShouldProceedOrCheckLimit_BasedOnRequestLimitRule(params RuleType[]? ruleTypes)
    {
        // Arrange
        var requestLimitRule = ruleTypes is not null ? new RateLimitRuleAttribute(ruleTypes) : null;
        var context = CreateActionContext(
            new Dictionary<string, string>
            {
                { "Identifier", Guid.NewGuid().ToString() },
                { "RegionType", "Us" }
            },
            requestLimitRule
        );

        _requestLimitExecutorMock
            .Setup(x => x.Execute(It.IsAny<RuleType[]>(), It.IsAny<Request>()))
            .Returns(true);

        var nextCalled = false;
        var next = CreateNextDelegate(context, () => nextCalled = true);

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task WithRequestLimitRules_And_LimitExceeded_ShouldReturn429StatusCode()
    {
        // Arrange
        var context = CreateActionContext(
            new Dictionary<string, string>
            {
                { "Identifier", Guid.NewGuid().ToString() },
                { "RegionType", "Us" }
            },
            new RateLimitRuleAttribute(RuleType.TimeSpanSinceLastCall, RuleType.RequestPerTimeSpan)
        );

        _requestLimitExecutorMock
            .Setup(x => x.Execute(It.IsAny<RuleType[]>(), It.IsAny<Request>()))
            .Returns(false);

        var next = CreateNextDelegate(context);

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        context.Result.Should().BeOfType<ContentResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
    }

    private static ActionExecutionDelegate CreateNextDelegate(ActionExecutingContext context, Action? onNextCalled = null)
    {
        return () =>
        {
            onNextCalled?.Invoke();
            
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new object()));
        };
    }

    private static ActionExecutingContext CreateActionContext(
        Dictionary<string, string> headers,
        RateLimitRuleAttribute? requestLimitRule = null)
    {
        var httpContext = new DefaultHttpContext();
        
        foreach (var header in headers)
        {
            httpContext.Request.Headers[header.Key] = new StringValues(header.Value);
        }

        var actionDescriptor = new ControllerActionDescriptor
        {
            EndpointMetadata = requestLimitRule is not null ? [requestLimitRule] : []
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(), new object());
    }
}