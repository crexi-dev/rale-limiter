using System;
using System.Reflection;
using System.Threading.Tasks;
using Example.Api.Controllers;
using Example.Api.Middleware;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using NUnit.Framework;
using RateLimiter.Attributes;

namespace RateLimiter.Tests;

[TestFixture]
public class RequestRateMiddlewareTest
{
    [Test]
    public async Task InvokeAsync_AllowRequest_RateLimitNotExceeded()
    {
        // arrange
        var next = A.Fake<RequestDelegate>();
        var rrm = new RequestRateMiddleware(next);
        var rateLimitManager = A.Fake<IRateLimitManager>();
        A.CallTo(() => rateLimitManager.IsRequestAllowed(A<IRateLimit>.Ignored, A<string>.Ignored)).Returns(false);

        // act
        var context = new DefaultHttpContext();
        context.SetEndpoint(new Endpoint(null, GetMetaData(), "Get"));
        await rrm.InvokeAsync(context, rateLimitManager);

        // assert
        Assert.AreEqual(403, context.Response.StatusCode);
    }

    [Test]
    public async Task InvokeAsync_BlockRequest_RateLimitExceeded()
    {
        // arrange
        var next = A.Fake<RequestDelegate>();
        var rrm = new RequestRateMiddleware(next);
        var rateLimitManager = A.Fake<IRateLimitManager>();
        A.CallTo(() => rateLimitManager.IsRequestAllowed(A<IRateLimit>.Ignored, A<string>.Ignored)).Returns(true);

        // act
        var context = new DefaultHttpContext();
        context.SetEndpoint(new Endpoint(null, GetMetaData(), "Get"));
        await rrm.InvokeAsync(context, rateLimitManager);

        // assert
        Assert.AreEqual(200, context.Response.StatusCode);
    }


    private EndpointMetadataCollection GetMetaData()
    {
        return new EndpointMetadataCollection(
            new ControllerActionDescriptor
            {
                MethodInfo = typeof(WeatherForecastController).GetMethod("Get"),
                ControllerTypeInfo = typeof(WeatherForecastController).GetTypeInfo(),
                ActionName = "Get",
                ControllerName = "WeatherForecastController"
            },
            new RequestsPerTimespanAttribute(5, 1)
        );
    }
}



