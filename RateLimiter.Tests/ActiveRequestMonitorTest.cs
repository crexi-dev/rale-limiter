namespace Crexi.RateLimiter.Tests;

[TestFixture]
[TestOf(typeof(ActiveRequestMonitor))]
public class ActiveRequestMonitorTest
{

    [Test]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(10000)]
    public void ActiveOpenRequestCount_IsCorrect(int requestCount)
    {
        ClientRequestTracker requestTracker = new();
        
        var requests = TestDataHelper.GetClientRequests(requestCount);
        var clientId = requests.First().ClientId;
        foreach (var request in requests)
        {
            requestTracker.AddRequest(request);
        }

        var actualActiveRequests = requestTracker.GetActiveRequestCount(clientId);
        Assert.That(actualActiveRequests, Is.EqualTo(requestCount));
    }
    
    
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(10000)]
    public void ClosedRequestCount_IsZero(int requestCount)
    {
        ClientRequestTracker requestTracker = new();
        
        var requests = TestDataHelper.GetClientRequests(requestCount);
        var clientId = requests.First().ClientId;
        foreach (var request in requests)
        {
            requestTracker.AddRequest(request);
            requestTracker.EndRequest(clientId, request.RequestId);
        }

        var actualActiveRequests = requestTracker.GetActiveRequestCount(clientId);
        Assert.That(actualActiveRequests, Is.EqualTo(0));
    }
}