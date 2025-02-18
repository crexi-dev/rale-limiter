using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using System.Net;

using Xunit;
using Xunit.Abstractions;

namespace RateLimiter.Tests.Integration
{
    public class WeatherForecastControllerTests(ITestOutputHelper output)
    {
        [Fact]
        public async Task GetAsync_ReturnsOk()
        {
            var factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();
            var response = await client.GetAsync("WeatherForecast");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Represents many unique clients (US and EU) making many calls to our API
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsync_WhenAllTokensAreUnique_RequestsAreNotRateLimited()
        {
            // arrange
            var factory = new WebApplicationFactory<Program>();

            var tasks = new List<Task<HttpResponseMessage>>();

            for (var i = 0; i < 10_000; i++)
            {
                var client = factory.CreateClient();
                client.DefaultRequestHeaders.Add("x-crexi-token",
                    i % 2 == 0 ? $"US-{Guid.NewGuid()}" : $"EU-{Guid.NewGuid()}");

                tasks.Add(client.GetAsync("WeatherForecast"));
            }

            // act
            await Task.WhenAll(tasks);

            // assert
            var limited = tasks.Any(t => !t.Result.IsSuccessStatusCode);
            limited.Should().BeFalse(because: "all clients have unique tokens");
        }

        /// <summary>
        /// Represents two clients (one US and one EU) making many calls to our API
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsync_WhenTokensAreNotUnique_RequestsAreRateLimited()
        {
            // arrange
            var factory = new WebApplicationFactory<Program>();

            var tasks = new List<Task<HttpResponseMessage>>();

            var usToken = $"US-{Guid.NewGuid()}";
            var euToken = $"EU-{Guid.NewGuid()}";

            for (var i = 0; i < 10_000; i++)
            {
                var client = factory.CreateClient();
                client.DefaultRequestHeaders.Add("x-crexi-token",
                    i % 2 == 0 ? usToken : euToken);
                tasks.Add(client.GetAsync("WeatherForecast"));
            }

            // act
            await Task.WhenAll(tasks);

            // assert
            var allowed = tasks.Count(t => t.Result.IsSuccessStatusCode);
            var limited = tasks.Count(t => !t.Result.IsSuccessStatusCode);

            output.WriteLine($"Allowed: {allowed}\tLimited: {limited}");
            var percent = ((double)limited / tasks.Count)*100;
            percent.Should().BeApproximately(99, 2.0, because: "two clients are banging on the door");
        }
    }
}
