using NUnit.Framework;
using RateLimiter.Attributes;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void Requests_Per_Timespan_Exceeded()
	{
        //arrange
        var rlm = new RateLimitManager();

        //act
        var result = rlm.IsRequestAllowed(new RequestsPerTimespanAttribute(1, 2, "ErrorMsg"), "");

        //assert
        Assert.IsFalse(result);
	}
}