using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RateLimiter.Rules;

namespace RateLimiter
{
    public static class EndpointExtensions
    {
        public static void PopulateRateLimiterRules(
            this IEndpointRouteBuilder endpoints,
            IEnumerable<IRule> rules,
            IRuleService ruleService)
        {
            var resourcexRuleNames = new Dictionary<string, IEnumerable<string>>();
            var dataSource = endpoints.DataSources.First();
            foreach (var endpoint in dataSource.Endpoints)
            {
                if (endpoint == null) continue;

                string? resource = endpoint.GetResourceName();

                var metadata = endpoint.Metadata.GetMetadata<RuleEndpointMetadata>();

                if (metadata == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(resource))
                {
                    throw new Exception("Could not get a resource name for an endpoint with rate limiter rules");
                }

                resourcexRuleNames.Add(resource, metadata.Rules);
            }

            ruleService.AddResourceRules(rules, resourcexRuleNames);
        }

        public static string? GetResourceName(this Endpoint endpoint)
        {
            if (endpoint is RouteEndpoint routeEndpoint)
            {
                // Combine route template and HTTP method to create a unique key
                var httpMethods = routeEndpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods;
                if (httpMethods == null || httpMethods.Count() == 0)
                {
                    return null;
                }

                string routeTemplate = routeEndpoint!.RoutePattern!.RawText!;
                string uniqueKey = $"{httpMethods.First()}:{routeTemplate}";

                return uniqueKey;
            }

            throw new ArgumentException("endpoit is not a RouteEndpoint");
        }
    }
}
