using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using RateLimiter.Attributes;
using System.Diagnostics;

namespace RateLimiter.Tests
{
    [TestFixture]
    internal class RegionRateLimitAttributeTests
    {
        private RegionRateLimitAttribute _attribute;

        [SetUp]
        public void Setup()
        {
            // configure region limits: US - 2 requests/60 secs, EU - 10s cooldown
            _attribute = new RegionRateLimitAttribute(usLimit: 2, usWindowSeconds: 60, euCooldownSeconds: 10);
        }

        private async Task<ActionExecutingContext> CreateExecutingContext(string clientToken)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Client-Token"] = clientToken;

            var routeData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var filters = new List<IFilterMetadata>();
            var actionArguments = new Dictionary<string, object>();

            return new ActionExecutingContext(actionContext, filters, actionArguments, controller: null);
        }

        private ActionExecutionDelegate CreateNextDelegate()
        {
            // simulate delegate that represents the next stage in the pipeline.
            return () =>
            {
                // for simplicity just return a completed Task
                var httpContext = new DefaultHttpContext();
                var routeData = new RouteData();
                var actionDescriptor = new ControllerActionDescriptor();
                var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

                return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null));
            };
        }

        [Test]
        public async Task RegionRateLimit_US_BlocksAfterLimit()
        {
            var clientToken = "US-testtoken";

            // execute two allowed requests first
            for (int i = 0; i < 2; i++)
            {
                var executingContext = await CreateExecutingContext(clientToken);
                var nextDelegate = CreateNextDelegate();
                await _attribute.OnActionExecutionAsync(executingContext, nextDelegate);

                // if the limit hasn't been reached, context.Result should be null
                Assert.IsNull(executingContext.Result, "Request should be allowed under the limit.");
            }

            // 3rd request should exceed the limit
            var thirdContext = await CreateExecutingContext(clientToken);
            var thirdNextDelegate = CreateNextDelegate();
            await _attribute.OnActionExecutionAsync(thirdContext, thirdNextDelegate);

            Assert.IsNotNull(thirdContext.Result, "Third request should be blocked.");

            Assert.IsInstanceOf<Attributes.ContentResult>(thirdContext.Result, "Result should be our custom ContentResult.");

            if (thirdContext.Result is Attributes.ContentResult contentResult)
            {
                Assert.AreEqual(429, contentResult.StatusCodes);
                Assert.IsTrue(contentResult.Content.Contains("Too Many Requests"));
            }
        }
    }
}
