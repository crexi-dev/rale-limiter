using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;

namespace RateLimiter.Tests.API;

[TestFixture]
public class Api_Tests
{
    private WebApplicationFactory<Program> factory;
    private HttpClient client;

    [SetUp]
    public void Setup()
    {
        factory = new WebApplicationFactory<Program>();
        client = factory.CreateClient();
    }

    [Test]
    public async Task ResourceA_AllowsUpToLimit()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", "client1");

        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            var response = await client.GetAsync("/api/ResourceA");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        // Next request should be rate-limited
        var rateLimitedResponse = await client.GetAsync("/api/ResourceA");
        Assert.AreEqual(System.Net.HttpStatusCode.TooManyRequests, rateLimitedResponse.StatusCode);
    }

    [Test]
    public async Task ResourceB_EnforcesDelay()
    {
        // Arrange
        client.DefaultRequestHeaders.Add("Authorization", "client1");

        // First request
        var response1 = await client.GetAsync("/api/ResourceB");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response1.StatusCode);

        // Immediate second request should be rate-limited
        var response2 = await client.GetAsync("/api/ResourceB");
        Assert.AreEqual(System.Net.HttpStatusCode.TooManyRequests, response2.StatusCode);

        // Wait for delay
        await Task.Delay(10000); // 10 seconds

        var response3 = await client.GetAsync("/api/ResourceB");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response3.StatusCode);
    }

    [Test]
    public async Task ResourceD_AppliesGeoBasedRules()
    {
        // Arrange for US client
        client.DefaultRequestHeaders.Add("Authorization", "US123");

        // Act & Assert for US client
        for (int i = 0; i < 10; i++)
        {
            var response = await client.GetAsync("/api/ResourceD");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        var usRateLimitedResponse = await client.GetAsync("/api/ResourceD");
        Assert.AreEqual(System.Net.HttpStatusCode.TooManyRequests, usRateLimitedResponse.StatusCode);

        // Arrange for EU client
        _=client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Add("Authorization", "EU456");

        // First request
        var euResponse1 = await client.GetAsync("/api/ResourceD");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, euResponse1.StatusCode);

        // Immediate second request should be rate-limited
        var euResponse2 = await client.GetAsync("/api/ResourceD");
        Assert.AreEqual(System.Net.HttpStatusCode.TooManyRequests, euResponse2.StatusCode);

        // Wait for delay
        await Task.Delay(15000); // 15 seconds

        var euResponse3 = await client.GetAsync("/api/ResourceD");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, euResponse3.StatusCode);
    }
}