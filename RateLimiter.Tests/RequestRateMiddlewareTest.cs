using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Example.Api.Attributes;
using Example.Api.Controllers;
using Example.Api.Managers;
using Example.Api.Middleware;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class RequestRateMiddlewareTest
{
    [Test]
    public async Task InvokeAsync_Unauthorized_Request_No_UserData_Provided()
    {
        // arrange
        var next = A.Fake<RequestDelegate>();
        var rrm = new RequestRateMiddleware(next);
        var rateLimitManager = A.Fake<IRateLimitManager>();
        
        A.CallTo(() => rateLimitManager.CanPerformRequest(A<string>.Ignored, A<UserToken>.Ignored)).Returns(false);

        // act
        var context = new DefaultHttpContext();
        context.SetEndpoint(new Endpoint(null, GetMetaData(), "Get"));
        await rrm.InvokeAsync(context, rateLimitManager);

        // assert
        Assert.AreEqual(401, context.Response.StatusCode);
    }


    [Test]
    public async Task InvokeAsync_AllowRequest_RateLimit_Not_Exceeded()
    {
        // arrange
        var next = A.Fake<RequestDelegate>();
        var rrm = new RequestRateMiddleware(next);
        var rateLimitManager = A.Fake<IRateLimitManager>();
        A.CallTo(() => rateLimitManager.CanPerformRequest(A<string>.Ignored, A<UserToken>.Ignored)).Returns(false);

        // act
        var context = new DefaultHttpContext();
        context.Request.Headers.Add("UserName", "123456");
        context.Request.Headers.Add("Origin", "USA");
        context.SetEndpoint(new Endpoint(null, GetMetaData(), "Get"));
        await rrm.InvokeAsync(context, rateLimitManager);

        // assert
        Assert.AreEqual(403, context.Response.StatusCode);
    }

    [Test]
    public async Task InvokeAsync_BlockRequest_RateLimit_Exceeded()
    {
        // arrange
        var next = A.Fake<RequestDelegate>();
        var rrm = new RequestRateMiddleware(next);
        var rateLimitManager = A.Fake<IRateLimitManager>();
        A.CallTo(() => rateLimitManager.CanPerformRequest(A<string>.Ignored, A<UserToken>.Ignored)).Returns(true);

        // act
        var context = new DefaultHttpContext();
        context.Request.Headers.Add("UserName", "123456");
        context.Request.Headers.Add("Origin", "USA");
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
            new RequestsPerTimespanAttribute(5, 1, "Error")
        );
    }

}



