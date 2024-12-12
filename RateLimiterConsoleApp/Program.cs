using RateLimiter;
using RateLimiter.Interfaces;
using RateLimiter.Repositories;
using RateLimiter.Rules;

namespace RateLimiterConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string resource1 = "Resource1";
            const string resource2 = "Resource2";
            const string resource3 = "Resource3";
            const string regionUS = "US";
            const string regionEU = "EU";
            const string regionASIA = "ASIA";

            try
            {
                //Setup global resource rate limit
                IResourceRateLimit _resourceRateLimit = new ResourceRateLimit();

                //Setup Resource1 to US Region with the rule maximum 2 requests per hour
                _resourceRateLimit.AddRuleResource(resource1 + "." + regionUS, new RuleXRequestsPerTimespan(2, new TimeSpan(1, 0, 0)));

                //Setup Resource2 to EU Region with the rule where the new request is success if the last previous recorded request was 10 minutes ago
                _resourceRateLimit.AddRuleResource(resource2 + "." + regionEU, new RuleTimePassedSinceLastCall(new TimeSpan(0, 10, 0)));

                //Setup Resource3 to Asia Region with both rules configured as the following:
                //Rule RuleXRequestsPerTimespan allows two requests within 30 seconds
                //Rule RuleTimePassedSinceLastCall allows a request at least 1 seconds later than the previously recorded request 
                _resourceRateLimit.AddRuleResource(resource3 + "." + regionASIA, new RuleXRequestsPerTimespan(2, new TimeSpan(0, 0, 30)));
                _resourceRateLimit.AddRuleResource(resource3 + "." + regionASIA, new RuleTimePassedSinceLastCall(new TimeSpan(0, 0, 1)));

                //Setting up Client US
                IClient client = new Client("US.xxxx");

                //the first two requests should succeed and the third request should fail
                for (int i = 0; i < 3; i++)
                {
                    var result = _resourceRateLimit.CheckRule(resource1, client);

                    if (result != null)
                    {
                        if (result.IsSuccess)
                        {
                            client.AddRequest();

                            Console.WriteLine($"Region {client.ReturnRegion()} request {resource1} succeeded on {DateTime.UtcNow}.\n");
                        }
                        else
                            Console.WriteLine($"Region {client.ReturnRegion()} request {resource1} failed on {DateTime.UtcNow}.\nRule failed: {result.RuleName}\nMessage:{result.Message}\n");
                    }
                    else
                        Console.WriteLine($"Region {client.ReturnRegion()} request {resource1} failed on rules checking.");

                    Thread.Sleep(1000);
                }

                //Setting up Client EU
                client = new Client("EU.xxxx");

                //the first request should succeed and the second request should fail
                for (int i = 0; i < 2; i++)
                {
                    var result = _resourceRateLimit.CheckRule(resource2, client);

                    if (result != null)
                    {
                        if (result.IsSuccess)
                        {
                            client.AddRequest();

                            Console.WriteLine($"Region {client.ReturnRegion()} request {resource2} succeeded on {DateTime.UtcNow}.\n");
                        }
                        else
                            Console.WriteLine($"Region {client.ReturnRegion()} request {resource2} failed on {DateTime.UtcNow}.\nRule failed: {result.RuleName}\nMessage:{result.Message}\n");
                    }
                    else
                        Console.WriteLine($"Region {client.ReturnRegion()} request {resource2} failed on rules checking.");

                    Thread.Sleep(1000);
                }

                //Setting up Client ASIA. Testing the failure of Rule RuleXRequestsPerTimespan
                client = new Client("ASIA.xxxx");

                //the first two requests should succeed and the third request should fail
                for (int i = 0; i < 3; i++)
                {
                    var result = _resourceRateLimit.CheckRule(resource3, client);

                    if (result != null)
                    {
                        if (result.IsSuccess)
                        {
                            client.AddRequest();

                            Console.WriteLine($"Region {client.ReturnRegion()} request {resource3} succeeded on {DateTime.UtcNow}.\n");
                        }
                        else
                            Console.WriteLine($"Region {client.ReturnRegion()} request {resource3} failed on {DateTime.UtcNow}.\nRule failed: {result.RuleName}\nMessage:{result.Message}\n");
                    }
                    else
                        Console.WriteLine($"Region {client.ReturnRegion()} request {resource3} failed on rules checking.");

                    Thread.Sleep(1000);
                }

                //Setting up Client ASIA. Testing the failure of Rule RuleTimePassedSinceLastCall
                client = new Client("ASIA.xxxx");

                //the first rquest should succeed and the second request should fail
                for (int i = 0; i < 2; i++)
                {
                    var result = _resourceRateLimit.CheckRule(resource3, client);

                    if (result != null)
                    {
                        if (result.IsSuccess)
                        {
                            client.AddRequest();

                            Console.WriteLine($"Region {client.ReturnRegion()} request {resource3} succeeded on {DateTime.UtcNow}.\n");
                        }
                        else
                            Console.WriteLine($"Region {client.ReturnRegion()} request {resource3} failed on {DateTime.UtcNow}.\nRule failed: {result.RuleName}\nMessage:{result.Message}\n");
                    }
                    else
                        Console.WriteLine($"Region {client.ReturnRegion()} request {resource3} failed on rules checking.");
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"An error has been detected: {ex.Message}");
            }
        }
    }
}
