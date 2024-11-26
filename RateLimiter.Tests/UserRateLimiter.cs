using Moq;
using NUnit.Framework;
using RateLimiter.ExampleUserSetup;
using System;
using System.Threading;

namespace RateLimiter.Tests;

[TestFixture]
public class UserRateLimiterTest
{
    [Test]
    public void Test_User_Resource()
    {
        var rateLimiter = new RejectRateLimiter(2,TimeSpan.FromSeconds(5));
        
        var userRateLimiter = new UsersRateLimiter(rateLimiter);

        var mockUser = new Mock<IUser>();
        mockUser.SetupGet(x => x.Name).Returns("Mary");
        var user = mockUser.Object;
        
        Assert.That(userRateLimiter.CheckRequest(user, "DoSomething"), Is.True);
        Assert.That(userRateLimiter.CheckRequest(user, "DoSomething"), Is.True);
        Assert.That(userRateLimiter.CheckRequest(user, "DoSomething"), Is.False);
    }
}
