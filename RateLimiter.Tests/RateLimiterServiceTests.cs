using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Interface;
using RateLimiter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimiterServiceTests
    {
        private ILogger<RateLimiterService> _logger;

        public RateLimiterServiceTests()
        { 
            _logger = new Mock <ILogger<RateLimiterService>>().Object;
        }

        [Test]
        public void RateLimiterService_Has_Access()
        {
            var rateLimiterRepostoryMock = new Mock<IRateLimiterRepository>();
            rateLimiterRepostoryMock
                .Setup(x => x.Get(It.IsAny<RequestDTO>())).Returns(new Request());

            var validator = new Mock<IRequestLimitValidator>();
            var reqeust = new Request();
            validator.Setup(x => x.Validate(reqeust)).Returns(true);
            var service = new RateLimiterService(_logger, rateLimiterRepostoryMock.Object, validator.Object);

            service.Validate(new RequestDTO());




        }

    }
}
