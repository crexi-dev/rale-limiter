using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using RateLimiter.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace RateLimiter.Tests.Middleware
{
    public class RateLimiterMiddlewareTests
    {
        private RateLimiterMiddleware _middleware;
        private RateLimiterManager _manager;
        private DefaultHttpContext _context;

        private const string TEST_RESOURCE = "/api/test/resource";
        private const string CLIENT_A = "ClientA";
        private const string HEADER_CLIENT_ID = "X-Client-Id";
        private const string CONTENT_TYPE_JSON = "application/json";
        private const string SWAGGER_PATH = "/swagger/index.html";
        private const string HEALTH_CHECK_PATH = "/health";

        private const int HTTP_STATUS_OK = 200;
        private const int HTTP_STATUS_BAD_REQUEST = 400;
        private const int HTTP_STATUS_TOO_MANY_REQUESTS = 429;

        [SetUp]
        public void Setup()
        {
            // Initialize with empty config list - specific tests will create their own configs
            _manager = new RateLimiterManager(new List<ClientRateLimitConfig>());
            _middleware = new RateLimiterMiddleware(
                next: (context) => Task.CompletedTask,
                _manager
            );
            _context = new DefaultHttpContext();
        }

        [Test]
        public async Task GivenSwaggerRequest_WhenInvoked_SkipsRateLimiting()
        {
            _context.Request.Path = SWAGGER_PATH;

            await _middleware.Invoke(_context);

            Assert.That(_context.Response.StatusCode, Is.EqualTo(HTTP_STATUS_OK));
        }

        [Test]
        public async Task GivenHealthCheckRequest_WhenInvoked_SkipsRateLimiting()
        {
            _context.Request.Path = HEALTH_CHECK_PATH;

            await _middleware.Invoke(_context);

            Assert.That(_context.Response.StatusCode, Is.EqualTo(HTTP_STATUS_OK));
        }

        [Test]
        public async Task GivenMissingClientId_WhenInvoked_ReturnsBadRequest()
        {
            _context.Request.Path = TEST_RESOURCE;

            await _middleware.Invoke(_context);

            Assert.Multiple(() =>
            {
                Assert.That(_context.Response.StatusCode, Is.EqualTo(HTTP_STATUS_BAD_REQUEST));
                Assert.That(_context.Response.ContentType, Is.EqualTo(CONTENT_TYPE_JSON));
            });
        }

        [Test]
        public async Task GivenRateLimitExceeded_WhenInvoked_ReturnsTooManyRequests()
        {
            // Create a new manager instance with the rate limit configuration
            var config = new ClientRateLimitConfig
            {
                ClientId = CLIENT_A,
                ResourceLimits = new List<ResourceRateLimitConfig>
                {
                    new ResourceRateLimitConfig
                    {
                        Resource = TEST_RESOURCE,
                        Rules = new List<IRateLimitRule>
                        {
                            new FixedWindowRateLimit(1, TimeSpan.FromSeconds(5))
                        }
                    }
                }
            };

            // Create new manager with the config
            _manager = new RateLimiterManager(new List<ClientRateLimitConfig> { config });

            // Create new middleware with the updated manager
            _middleware = new RateLimiterMiddleware(
                next: (context) => Task.CompletedTask,
                _manager
            );

            _context.Request.Path = TEST_RESOURCE;
            _context.Request.Headers[HEADER_CLIENT_ID] = CLIENT_A;

            // First request should succeed
            await _middleware.Invoke(_context);
            Assert.That(_context.Response.StatusCode, Is.EqualTo(HTTP_STATUS_OK));

            // Reset context for second request
            _context = new DefaultHttpContext();
            _context.Request.Path = TEST_RESOURCE;
            _context.Request.Headers[HEADER_CLIENT_ID] = CLIENT_A;

            // Second request should fail
            await _middleware.Invoke(_context);
            Assert.Multiple(() =>
            {
                Assert.That(_context.Response.StatusCode, Is.EqualTo(HTTP_STATUS_TOO_MANY_REQUESTS));
                Assert.That(_context.Response.Headers.ContainsKey("Retry-After"), Is.True);
            });
        }
    }
}