using System;
using System.Linq;
using Example.Api.Attributes;
using Example.Api.Managers;
using NUnit.Framework;
using System.Reflection;
using Example.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using FakeItEasy;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{

    [Test]
    public void Register_Methods_As_Resources()
    {
        //arrange
        var rlm = new RateLimitManager();
        var controllers = Assembly.GetAssembly(typeof(WeatherForecastController)).GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type));
        var method = controllers.First().GetMethods().First(m => m.GetCustomAttributes(typeof(IRateLimitAttribute), true).Any()) ;

        //act & assert
        rlm.RegisterResource("WeatherForecastController.Get", method);
        Assert.Pass();
    }

    [Test]
    public void Register_Double_Methods_As_Resources()
    {
        //arrange
        var rlm = new RateLimitManager();
        var controllers = Assembly.GetAssembly(typeof(WeatherForecastController)).GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type));
        var method = controllers.First().GetMethods().First(m => m.GetCustomAttributes(typeof(IRateLimitAttribute), true).Any());

        //act 
        rlm.RegisterResource("WeatherForecastController.Get", method);

        // assert
        Assert.Catch<InvalidOperationException>(() => rlm.RegisterResource("WeatherForecastController.Get", method));
    }



    [Test]
    public void Requests_Per_Timespan_Exceeded()
    {
        //arrange
        var rlm = GetManager();

        //act
        var firstResult = rlm.CanPerformRequest("WeatherForecastController.Get", new UserToken("1234567", "USA"));
        var secondResult = rlm.CanPerformRequest("WeatherForecastController.Get", new UserToken("1234567", "USA"));

        //assert
        Assert.IsTrue(firstResult);
        Assert.IsFalse(secondResult);
    }


    [Test]
    public void Requests_Per_Timespan_Not_Exceeded()
    {
        //arrange
        var rlm = GetManager();

        //act
        var result = rlm.CanPerformRequest("WeatherForecastController.Get", new UserToken("1234567", "USA"));

        //assert
        Assert.IsTrue(result);
    }

    [Test]
    public void Cooldown_Time_InProgress()
    {
        //arrange
        var rlm = GetManager();

        //act
        var firstCall = rlm.CanPerformRequest("WeatherForecastController.Get", new UserToken("1234567", "USA"));
        var secondCall = rlm.CanPerformRequest("WeatherForecastController.Get", new UserToken("1234567", "USA"));

        //assert
        Assert.IsTrue(firstCall);
        Assert.IsFalse(secondCall);
    }



    //[Test]
    //public void Requests_Per_Timespan_Succeed()
    //{
    //    //arrange
    //    var rlm = new RateLimitManager();

    //    //act
    //    var result = rlm.AddNewRequest(new RequestsPerTimespanAttribute(1, 2, "ErrorMsg"), new UserToken("1234567", "USA"));

    //    //assert
    //    Assert.IsTrue(result);
    //}


    private static IRateLimitManager GetManager()
    {
        var limitManager = new RateLimitManager();
        var controllers = Assembly.GetAssembly(typeof(WeatherForecastController)).GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type));
        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.GetCustomAttributes(typeof(IRateLimitAttribute), true).Any());
            foreach (var method in methods)
            {
                var fullName = method.DeclaringType.Name;
                limitManager.RegisterResource($"{fullName}.{method.Name}", method);
            }
        }

        return limitManager;
    }
}