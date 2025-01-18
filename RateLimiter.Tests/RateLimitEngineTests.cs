using System.Linq;

namespace Crexi.RateLimiter.Tests;

[TestFixture]
[TestOf(typeof(RateLimitEngine))]
public class RateLimitEngineTests
{
    [Test]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(10000)]
    public void RateLimitEngine_NoPolicies_NRequests_UnderLimit(int requestCount)
    {
        var requests = TestDataHelper.GetClientRequests(requestCount);
        var requestHistory = new ConcurrentQueue<ClientRequest>();
        
        foreach (var request in requests )
        {
            var result = new RateLimitEngine().CheckClientRequest(request, requestHistory, null);
            requestHistory.Enqueue(request);
            
            Assert.True(result.IsAllowed);
            Assert.IsNull(result.PassingPolicyNames);
            Assert.IsNull(result.PassingPolicyNames);
        }
    }
    
    [Test]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(10000)]
    public void RateLimitEngine_SlidingWindowPolicies_NRequests_UnderLimit(int requestCount)
    {
        var requests = TestDataHelper.GetClientRequests(requestCount);
        var requestHistory = new ConcurrentQueue<ClientRequest>();
        var policy = new RateLimitPolicySettings
        {
            PolicyName = nameof(RateLimitEngine_SlidingWindowPolicies_NRequests_UnderLimit),
            PolicyType = PolicyType.SlidingWindow,
            Limit = Int32.MaxValue,
            TimeSpanWindow = TimeSpan.FromMinutes(5),
        };
        
        var policies = new List<RateLimitPolicySettings>() { policy };
        
        foreach (var request in requests )
        {
            var result = new RateLimitEngine().CheckClientRequest(request, requestHistory, policies);
            requestHistory.Enqueue(request);
            
            Assert.True(result.IsAllowed);
            Assert.Contains(policy.PolicyName, result.PassingPolicyNames);
        }
    }
    
    [Test]
    [TestCase(10, 5)]
    [TestCase(101, 100)]
    [TestCase(10000, 9000)]
    public void RateLimitEngine_SlidingWindowPolicies_OverLimit(int requestCount, int limit)
    {
        var requests = TestDataHelper.GetClientRequests(requestCount);
        var requestHistory = new ConcurrentQueue<ClientRequest>();
        var policy = new RateLimitPolicySettings
        {
            PolicyName = nameof(RateLimitEngine_SlidingWindowPolicies_NRequests_UnderLimit),
            PolicyType = PolicyType.SlidingWindow,
            Limit = limit,
            TimeSpanWindow = TimeSpan.FromMinutes(5),
        };
        
        var policies = new List<RateLimitPolicySettings>() { policy };

        var requestsSent = 0;
        foreach (var request in requests)
        {
            var result = new RateLimitEngine().CheckClientRequest(request, requestHistory, policies);
            requestHistory.Enqueue(request);

            if (requestsSent <= limit)
            {
                continue;
            }
            
            Assert.False(result.IsAllowed);
            Assert.Contains(policy.PolicyName, result.FailingPolicyNames);
        }
    }
}