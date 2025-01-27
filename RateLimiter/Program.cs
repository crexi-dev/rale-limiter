
using RateLimiter.Rules;
using System;
using System.Threading;

namespace RateLimiter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rule = new XRequestsPerTimespanRule(5, TimeSpan.FromSeconds(10));
            var clientId = "client1";
            var resource = "api/resource";

            Console.WriteLine("Rate Limiter Console App");
            Console.WriteLine("Testing rate limiter with 7 requests...");

            for (int i = 0; i < 7; i++)
            {
                var allowed = rule.IsRequestAllowed(clientId, resource);
                Console.WriteLine($"Request {i + 1}: {(allowed ? "Allowed" : "Blocked")}");
                Thread.Sleep(1000); // Simulate 1-second delay between requests
            }
        }
    }
}
