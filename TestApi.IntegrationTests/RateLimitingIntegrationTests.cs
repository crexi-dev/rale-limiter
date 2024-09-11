using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RateLimiter.Rules;
using TestApi.Common;

namespace TestApi.IntegrationTests
{
    public class RateLimitingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private const string ClientToken1 = "client-1";
        private const string SlidingWindowToken = "sliding-window-test-token";

        public RateLimitingIntegrationTests(WebApplicationFactory<Program> factory)
        {
            var mockRateLimitRuleRepository = new Mock<IRateLimitRuleRepository>();
            mockRateLimitRuleRepository
                .Setup(repo => repo.GetRulesByClientId(It.Is<string>(id => id == ClientToken1)))
                .ReturnsAsync(new List<RateLimitRuleEntity>
                {
                    new RateLimitRuleEntity
                    {
                        ClientId = ClientToken1,
                        Resource = Routes.ResourceA,
                        RuleType = RuleType.RequestCount,
                        MaxRequests = 5,
                        TimeSpan = TimeSpan.FromSeconds(5)
                    }
                });
            mockRateLimitRuleRepository
                .Setup(repo => repo.GetRulesByClientId(It.Is<string>(id => id == SlidingWindowToken)))
                .ReturnsAsync(new List<RateLimitRuleEntity>
                {
                    new()
                    {
                        ClientId = SlidingWindowToken,
                        Resource = Routes.ResourceA,
                        RuleType = RuleType.RequestCount,
                        MaxRequests = 1,
                        TimeSpan = TimeSpan.FromSeconds(1)
                    }
                });

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IRateLimitRuleRepository));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddSingleton(mockRateLimitRuleRepository.Object);
                    services.BuildServiceProvider();
                });
            });
        }

        [Fact]
        public async Task RateLimiter_Should_Allow_Requests_Within_Limit()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", ClientToken1);

            // Act
            var response1 = await client.GetAsync(Routes.ResourceA);
            var response2 = await client.GetAsync(Routes.ResourceA);

            // Assert
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RateLimiter_Should_Enforce_Rate_Limit_And_Reject_Excess_Requests()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", ClientToken1);

            // Act
            for (int i = 0; i < 5; i++)
            {
                await client.GetAsync(Routes.ResourceA); // Assuming limit is 5 requests
            }

            var exceededResponse = await client.GetAsync(Routes.ResourceA);

            // Assert
            exceededResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
            var content = await exceededResponse.Content.ReadAsStringAsync();
            content.Should().Contain("Rate limit exceeded");
        }

        [Fact]
        public async Task Should_Handle_Missing_Authorization_And_Resource()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(Routes.ResourceA);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Authorization token is missing or empty.");
        }

        [Fact]
        public async Task Should_Apply_SlidingWindow_Rule_Correctly()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", SlidingWindowToken);

            // Act
            var firstResponse = await client.GetAsync(Routes.ResourceA);
            await Task.Delay(1000); // Simulate sliding window effect
            var secondResponse = await client.GetAsync(Routes.ResourceA);
            await Task.Delay(1000); // Simulate window expiry
            var thirdResponse = await client.GetAsync(Routes.ResourceA);

            // Assert
            firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            thirdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
