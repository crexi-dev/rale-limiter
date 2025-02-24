using NUnit.Framework;
using RateLimiter.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace RateLimiter.Tests.Controllers
{
    [TestFixture]
    public class RateLimitTestControllerTests
    {
        private RateLimitTestController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new RateLimitTestController();
        }

        [Test]
        public void GetResource1_ReturnsOk()
        {
            // Act
            var result = _controller.GetResource1() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public void GetResource2_ReturnsOk()
        {
            // Act
            var result = _controller.GetResource2() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}