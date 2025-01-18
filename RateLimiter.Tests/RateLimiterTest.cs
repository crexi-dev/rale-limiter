namespace Crexi.RateLimiter.Tests;

[TestFixture]
public class RateLimiterTest
{
	[Test]
	public void ConcurrentRequestsTest()
	{
		var policy = new RateLimitPolicySettings
		{
			PolicyName = nameof(ConcurrentRequestsTest),
			PolicyType = PolicyType.ConcurrentRequests,
			ApplyClientTagFilter = false,
			Limit = Int32.MaxValue
		};
		
		Assert.That(policy, Is.Not.Null);
	}
}