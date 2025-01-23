namespace Crexi.RateLimiter.Tests;

public static class TestDataHelper
{
    public readonly static List<string> TestRegions = ["US-CA", "US-AK", "CA-BC", "JP-26"];
    public readonly static List<string> TestSubscriptionLevels = ["FREE", "PREMIER", "PARTNER"];
   
    private static Random _random = new();

    public static ClientRequest GetClientRequest(string? regionCountryCode = null, string? subscriptionLevel = null)
    {
        return new ClientRequest()
        {
            ClientId = Guid.NewGuid().ToString(),
            RequestId = Guid.NewGuid().ToString(),
            RequestTime = DateTime.UtcNow,
            RegionCountryCode = regionCountryCode ?? TestRegions[_random.Next(TestRegions.Count)],
            SubscriptionLevel = subscriptionLevel ?? TestSubscriptionLevels[_random.Next(TestSubscriptionLevels.Count)]
        };
    }

    public static List<ClientRequest> GetClientRequests(int requestCount)
    {
        return GetClientRequests(requestCount, TestRegions[0], TestSubscriptionLevels[0]);
    }

    public static List<ClientRequest> GetClientRequests(int requestCount, string targetRegion, string targetSubscriptionLevel)
    {
        var clientId = Guid.NewGuid().ToString();
        var requests = new ClientRequest[requestCount]; // Use an array for thread safety
        Parallel.For(0, requestCount, index =>
        {
            requests[index] = GetClientRequest(targetRegion, targetSubscriptionLevel);
            requests[index].ClientId = clientId;
        });

        return requests.ToList();
    }
}