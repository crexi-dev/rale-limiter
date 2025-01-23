namespace Crexi.RateLimiter.Tests;

[TestFixture]
[TestOf(typeof(ClientRequestTracker))]
public class ClientRequestTrackerTest
{
    [Test]
    [TestCase(10000)]
    public void ClientRequestTracker_HasValidRequestQueue(int requestCount)
    {
        ClientRequestTracker tracker = new();
        
        var requests = TestDataHelper.GetClientRequests(requestCount);
        foreach (var request in requests)
        {
            tracker.AddRequest(request);
        }
        
        var clientId = requests.First().ClientId;
        var actualRequestQueue = tracker.GetRequestQueue(clientId);
        
        Assert.That(actualRequestQueue.Count, Is.EqualTo(requestCount));
        Assert.That(actualRequestQueue.First().ClientId, Is.EqualTo(clientId));
    }
    
    [Test]
    [TestCase(10000)]
    public void ClientRequestTracker_HasActiveRequests(int requestCount)
    {
        ClientRequestTracker tracker = new();
        
        var requests = TestDataHelper.GetClientRequests(requestCount);
        foreach (var request in requests)
        {
            tracker.AddRequest(request);
        }
        
        var clientId = requests.First().ClientId;
        var actualActiveRequestCount = tracker.GetActiveRequestCount(clientId);
        
        Assert.That(actualActiveRequestCount, Is.EqualTo(requestCount));;
    }
}