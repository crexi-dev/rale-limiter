using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RateLimiter.Common;
using RateLimiter.Counter;
using RateLimiter.Rules;

namespace RateLimiter.Middleware
{
    public class RateLimiterMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IRuleService ruleService;
        private readonly IRequestCounter requestCounter;
        private readonly ITimeProvider timeProvider;
        private ILogger logger;

        public RateLimiterMiddleware(
            RequestDelegate next, 
            IRuleService ruleService, 
            IRequestCounter requestCounter,
            ITimeProvider timeProvider,
            ILogger<RateLimiterMiddleware> logger)
        {
            this.next = next;
            this.ruleService = ruleService;
            this.requestCounter = requestCounter;
            this.timeProvider = timeProvider;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var client = this.GetClient(context);

            if (client == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var resource = context.GetEndpoint()?.GetResourceName();

            if (!string.IsNullOrEmpty(resource))
            {
                var allowRequest = await this.ruleService.Allow(resource, client);

                if (!allowRequest)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    return;
                }
            }
            else
            {
                this.logger.LogWarning("Unable to determine resource name for path: {path}", context.Request.Path);
            }

            //record the request
            await this.requestCounter.RecordRequest(client.Id, this.timeProvider.Now());

            // Call the next delegate/middleware in the pipeline.
            await next(context);
        }

        /// <summary>
        /// Get client details from the claims (which come from the access token)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Client? GetClient(HttpContext context)
        {
            string? clientId = context.User.Claims.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))?.Value;
            string? countryCode = context.User.Claims.FirstOrDefault(c => c.Type.Equals("country_code", StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(countryCode))
            {
                return null;
            }

            return new Client(clientId, countryCode);

        }
    }
}
