namespace Crexi.RateLimiter.Tests;

public static class TestDataHelper
{
    public static ClientRequest GetClientRequest()
    {
        return new ClientRequest()
        {
            ClientId = Guid.NewGuid().ToString(),
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow,
            RegionCountryCode = "US-CA",
            SubscriptionLevel = "FREE"
        };
    }
    
    public static List<ClientRequest> GetClientRequests(int requestCount)
    {
        var requests = new List<ClientRequest>();
        for (var i = 0; i < requestCount; i++)
        {
            requests.Add(GetClientRequest());
        }
        
        return requests;
    }
}