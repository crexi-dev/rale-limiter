namespace Crexi.RateLimiter.Tests;

[TestFixture]
[TestOf(typeof(RateLimitEngine))]
public class RateLimitEngineConcurrentEvaluatorTest
{
    private NullLogger<IRateLimitEngine> _logger = new();
    
    [Test]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(10000)]
    public void ConcurrentRequest_UnderLimit(int requestCount)
    {
        ClientRequestTracker requestTracker = new();

        var requests = TestDataHelper.GetClientRequests(requestCount);
        var policy = new RateLimitPolicy
        {
            PolicyType = PolicyType.ConcurrentRequests,
            PolicyName = nameof(ConcurrentRequest_UnderLimit),
            Limit = Int32.MaxValue,
        };
    
        var policies = new List<RateLimitPolicy>() { policy };
    
        foreach (var request in requests )
        {
            var result = new RateLimitEngine().CheckClientRequest(request, requestTracker, policies, _logger);
            requestTracker.AddRequest(request);
            requestTracker.EndRequest(request.ClientId, request.RequestId);
            
            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.PassingPolicyNames, Does.Contain(policy.PolicyName));
        }
    }
    
    [Test]
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(10000)]
    public void ConcurrentRequest_OverLimit(int requestCount)
    {
        ClientRequestTracker requestTracker = new();

        var policy = new RateLimitPolicy
        {
            PolicyType = PolicyType.ConcurrentRequests,
            PolicyName = nameof(ConcurrentRequest_OverLimit),
            Limit = 5,
        };
        var policies = new List<RateLimitPolicy>() { policy };
        
        var requests = TestDataHelper.GetClientRequests(requestCount);
        for (var index = 0; index < requests.Count; index++)
        {
            var request = requests[index];
            var result = new RateLimitEngine().CheckClientRequest(request, requestTracker, policies, _logger);
            requestTracker.AddRequest(request);

            if (index < policy.Limit)
            {
                Assert.That(result.IsAllowed, Is.True);
                Assert.That(result.PassingPolicyNames, Does.Contain(policy.PolicyName));
            }
            else
            {
                Assert.That(result.IsAllowed, Is.False);
                Assert.That(result.FailingPolicyNames, Does.Contain(policy.PolicyName));
            }
        }
    }
}