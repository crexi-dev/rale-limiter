namespace Crexi.RateLimiter.Tests;

[TestFixture]
[TestOf(typeof(RateLimitEngine))]
public class RateLimitEngineNoPoliciesTest
{
    private NullLogger<IRateLimitEngine> _logger = new();
    
    [Test]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(10000)]
    public void NullPolicies_AllRequestsAllowed(int requestCount)
    {
        ClientRequestTracker tracker = new();
        List<RateLimitPolicy>? policies = null;
        
        var clientRequests = TestDataHelper.GetClientRequests(requestCount);
        foreach (var request in clientRequests)
        {
            var result = new RateLimitEngine().CheckClientRequest(request, tracker, policies, _logger);

            Assert.That(result.IsAllowed, Is.True);
            Assert.IsNull(result.PassingPolicyNames);
            Assert.IsNull(result.FailingPolicyNames);
        }
    }
    
    [Test]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(10000)]
    public void EmptyPolicies_AllRequestsAllowed(int requestCount)
    {
        ClientRequestTracker tracker = new();
        List<RateLimitPolicy> policies = new();
        
        var clientRequests = TestDataHelper.GetClientRequests(requestCount);
        foreach (var request in clientRequests)
        {
            var result = new RateLimitEngine().CheckClientRequest(request, tracker, policies, _logger);

            Assert.That(result.IsAllowed, Is.True);
            Assert.IsNull(result.PassingPolicyNames);
            Assert.IsNull(result.FailingPolicyNames);
        }
    }
}