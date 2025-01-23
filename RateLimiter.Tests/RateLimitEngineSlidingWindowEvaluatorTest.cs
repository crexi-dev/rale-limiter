namespace Crexi.RateLimiter.Tests;

[TestFixture]
[TestOf(typeof(RateLimitEngine))]
public class RateLimitEngineSlidingWindowEvaluatorTest
{
    private NullLogger<IRateLimitEngine> _logger = new();
    
    [Test]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(10000)]
    public void RateLimitEngine_SlidingWindowPolicies_NRequests_UnderLimit(int requestCount)
    {
        ClientRequestTracker requestTracker = new();
        
        var policies = new List<RateLimitPolicy>()
        {
            new RateLimitPolicy
            {
                PolicyName = nameof(RateLimitEngine_SlidingWindowPolicies_NRequests_UnderLimit),
                PolicyType = PolicyType.SlidingWindow,
                Limit = Int32.MaxValue,
                TimeSpanWindow = TimeSpan.FromMinutes(5),
            }
        };
        
        var requests = TestDataHelper.GetClientRequests(requestCount);
        foreach (var request in requests )
        {
            var result = new RateLimitEngine().CheckClientRequest(request, requestTracker, policies, _logger);
            requestTracker.AddRequest(request);
            
            Assert.That(result.IsAllowed, Is.True);
            Assert.That(result.PassingPolicyNames, Does.Contain(policies.First().PolicyName));
        }
    }
    
    [Test]
    [TestCase(10, 5)]
    [TestCase(101, 100)]
    [TestCase(10000, 9000)]
    public void RateLimitEngine_SlidingWindowPolicies_OverLimit(int requestCount, int limit)
    {
        var policies = new List<RateLimitPolicy>()
        {
            new RateLimitPolicy
            {
                PolicyName = nameof(RateLimitEngine_SlidingWindowPolicies_NRequests_UnderLimit),
                PolicyType = PolicyType.SlidingWindow,
                Limit = limit,
                TimeSpanWindow = TimeSpan.FromMinutes(5),
            }
        };
        
        ClientRequestTracker requestTracker = new();
        var requests = TestDataHelper.GetClientRequests(requestCount);
        foreach (var request in requests)
        {
            var result = new RateLimitEngine().CheckClientRequest(request, requestTracker, policies, _logger);
            requestTracker.AddRequest(request);

            if (requestTracker.GetActiveRequestCount(request.ClientId) <= limit)
            {
                continue;
            }
            
            Assert.That(result.IsAllowed, Is.False);
            Assert.That(result.FailingPolicyNames, Does.Contain(policies.First().PolicyName));
        }
    }
    
    [Test]
    [TestCase(10, 5)]
    [TestCase(101, 100)]
    [TestCase(10000, 9000)]
    public void RateLimitEngine_SlidingWindowPolicies_ClientFilter_OverLimit(int requestCount, long limitOverride)
    {
        const string testRegion = "US-CA";
        const string testSubscriptionLevel = "FREE";
        
        var policy = new RateLimitPolicy
        {
            PolicyName = nameof(RateLimitEngine_SlidingWindowPolicies_NRequests_UnderLimit),
            PolicyType = PolicyType.SlidingWindow,
            Limit = 1,
            TimeSpanWindow = TimeSpan.FromMinutes(5),
            ApplyClientTagFilter = true,
            ClientFilterGroups =
            [
                new ClientFilterGroup
                {
                    LimitOverride = limitOverride,
                    ClientFilters =
                    [
                        new ClientFilter
                        {
                            PropertyName = "RegionCountryCode",
                            HasTargetValue = true,
                            TargetValue = testRegion
                        },
                        new ClientFilter
                        {
                            PropertyName = "SubscriptionLevel",
                            HasTargetValue = true,
                            TargetValue = testSubscriptionLevel
                        }
                    ]
                },
                new ClientFilterGroup()
                {
                    LimitOverride = 10000,
                    ClientFilters = 
                    [
                        new ClientFilter
                        {
                            PropertyName = "RegionCountryCode",
                            HasTargetValue = true,
                            TargetValue = "US-NV"
                        }
                    ]
                }
            ]
        };

        var policies = new List<RateLimitPolicy>() { policy };
        
        var requests = TestDataHelper.GetClientRequests(requestCount, testRegion, testSubscriptionLevel);
        ClientRequestTracker requestTracker= new();
        
        foreach (var request in requests)
        {
            var result = new RateLimitEngine().CheckClientRequest(request, requestTracker, policies, _logger);
            requestTracker.AddRequest(request);

            if (requestTracker.GetRequestQueue(request.ClientId).Count <= limitOverride)
            {
                Assert.That(result.IsAllowed, Is.True);
            }
            else
            {
                Assert.That(result.IsAllowed, Is.False);
                Assert.That(result.FailingPolicyNames, Does.Contain(policy.PolicyName));
            }
        }
    }
}