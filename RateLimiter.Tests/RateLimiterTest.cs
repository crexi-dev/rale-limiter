using Microsoft.AspNetCore.Http;
using Moq;
using RateLimiter.Rules;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using RateLimiter.Configuration;
using System.Collections.Generic;
using RateLimiter.Policy;
using System.Linq;

namespace RateLimiter.Tests
{
    public class RateLimiterTest
    {
        private readonly Mock<IDistributedCache> _cacheMock;

        public RateLimiterTest()
        {
            _cacheMock = new Mock<IDistributedCache>();
        }

        #region Combined

        [Fact]
        public async Task EvaluateAll_ShouldAllow_WhenAllUnderLimit()
        {
            // Arrange
            List<string> configIpBlocked = ["192.168.1.1", "192.168.1.2"];
            FixedWindowConfig configFixed = new(5, 10);
            string current = "2";
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                      .ReturnsAsync(Encoding.UTF8.GetBytes(current));

            var ruleFixed = new FixedWindowRule(_cacheMock.Object, configFixed.Limit, configFixed.Seconds);
            var ruleIp = new IpBlacklistRule(configIpBlocked);
            var policy = new RateLimitPolicy();
            policy.AddRule(ruleFixed);
            policy.AddRule(ruleIp);

            var httpContext = GetHttpContext();

            // Act
            bool result = await policy.EvaluateAllAsync(httpContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EvaluateAll_ShouldNotAllow_BlockedIp()
        {
            // Arrange
            List<string> configIpBlocked = ["192.168.1.101"];
            FixedWindowConfig configFixed = new(5, 10);
            string current = "2";
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                      .ReturnsAsync(Encoding.UTF8.GetBytes(current));

            var ruleFixed = new FixedWindowRule(_cacheMock.Object, configFixed.Limit, configFixed.Seconds);
            var ruleIp = new IpBlacklistRule(configIpBlocked);
            var policy = new RateLimitPolicy();
            policy.AddRule(ruleFixed);
            policy.AddRule(ruleIp);

            var httpContext = GetHttpContext(configIpBlocked.First());

            // Act
            bool result = await policy.EvaluateAllAsync(httpContext);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Fixed

        [Fact]
        public async Task EvaluateFixed_ShouldAllow_WhenUnderLimit()
        {
            // Arrange
            FixedWindowConfig config = new(5, 10);
            string current = "2";
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                      .ReturnsAsync(Encoding.UTF8.GetBytes(current));

            var rule = new FixedWindowRule(_cacheMock.Object, config.Limit, config.Seconds);
            var httpContext = GetHttpContext();

            // Act
            bool result = await rule.EvaluateAsync(httpContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EvaluateFixed_ShouldNotAllow_WhenOverLimit()
        {
            // Arrange
            FixedWindowConfig config = new(2, 10);
            string current = "2";
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                      .ReturnsAsync(Encoding.UTF8.GetBytes(current));

            var rule = new FixedWindowRule(_cacheMock.Object, config.Limit, config.Seconds);
            var httpContext = GetHttpContext();

            // Act
            bool result = await rule.EvaluateAsync(httpContext);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task EvaluateFixed_ShouldAllow_WhenCacheEmpty()
        {
            // Arrange
            FixedWindowConfig config = new(5, 10);
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                      .ReturnsAsync(null as byte[]);

            var rule = new FixedWindowRule(_cacheMock.Object, config.Limit, config.Seconds);
            var httpContext = GetHttpContext();

            // Act
            bool result = await rule.EvaluateAsync(httpContext);

            // Assert
            Assert.True(result); // First request should pass
        }

        [Fact]
        public async Task EvaluateFixed_ShouldUpdateCache_WhenRequestMade()
        {
            // Arrange
            FixedWindowConfig config = new(5, 10);
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                      .ReturnsAsync(Encoding.UTF8.GetBytes("2")); // Simulate 2 requests so far

            var rule = new FixedWindowRule(_cacheMock.Object, config.Limit, config.Seconds);
            var httpContext = GetHttpContext();

            // Act
            bool result = await rule.EvaluateAsync(httpContext);

            // Assert
            Assert.True(result); // Request should pass
            _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
        }

        #endregion

        #region Geo

        [Fact]
        public async Task EvaluateGeo_ShouldAllow_WhenUnderLimit()
        {
            // Arrange
            GeoBasedConfig config = new(Country.EU, 10);
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                      .ReturnsAsync(null as byte[]);

            var rule = new GeoBasedRule(_cacheMock.Object, [config]);
            var httpContext = GetHttpContext();

            // Act
            bool result = await rule.EvaluateAsync(httpContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EvaluateGeo_ShouldNotAllow_WhenOverLimit()
        {
            // Arrange
            GeoBasedConfig config = new(Country.EU, 10);
            string current = "0";
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                      .ReturnsAsync(Encoding.UTF8.GetBytes(current));

            var rule = new GeoBasedRule(_cacheMock.Object, [config]);
            var httpContext = GetHttpContext();

            // Act
            bool result = await rule.EvaluateAsync(httpContext);

            // Assert
            Assert.False(result);
        }

        #endregion

        private static DefaultHttpContext GetHttpContext(string ip = "192.168.1.100")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse(ip);
            return httpContext;
        }
    }
}
