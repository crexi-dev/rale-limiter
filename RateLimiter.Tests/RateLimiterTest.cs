using System;
using NUnit.Framework;

namespace RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void FirstRequest_Should_BeAllowed()
	{
		throw new NotImplementedException();
	}

	[Test]
	public void SecondRequest_WithinSameWindow_Should_BeBlocked()
	{
		throw new NotImplementedException();
	}
}