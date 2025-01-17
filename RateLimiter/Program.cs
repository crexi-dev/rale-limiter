using Microsoft.Extensions.DependencyInjection;
using RateLimitingLibrary.Core.Interfaces;
using RateLimitingLibrary.Core.Models;
using System;
using System.Threading.Tasks;

namespace RateLimitingApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var serviceProvider = Startup.ConfigureServices();
            var rateLimiter = serviceProvider.GetRequiredService<IRateLimiter>();

            var requests = new[]
            {
                new ClientRequest { ClientToken = "Client1", Resource = "ResourceA", RequestTime = DateTime.UtcNow },
                new ClientRequest { ClientToken = "Client1", Resource = "ResourceA", RequestTime = DateTime.UtcNow.AddSeconds(10) }
            };

            foreach (var request in requests)
            {
                var result = await rateLimiter.EvaluateRequestAsync(request);
                Console.WriteLine($"Request for {request.Resource}: {result.Message}");
            }
        }
    }
}