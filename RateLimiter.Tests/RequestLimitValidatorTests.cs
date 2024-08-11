using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Interface;
using RateLimiter.Interface.Rule;
using RateLimiter.Model;
using RateLimiter.Rule.Request.LastCall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RequestLimitValidatorTests
    {
        private ILogger<RequestLimitValidator> _logger;
        public RequestLimitValidatorTests()
        {
            _logger = new Mock<ILogger<RequestLimitValidator>>().Object;
        }

        [Test]
        public void RequestLimitValidator_Region_Set_Corret_Number()
        {
            var testRule = new Mock<IRateLimiterRule>();
            testRule.Setup(x => x.SupportedRegion).Returns(new List<string> { "US" });
            testRule.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);

            var testRule1 = new Mock<IRateLimiterRule>();
            testRule1.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);

            var testRule2 = new Mock<IRateLimiterRule>();
            testRule2.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);

            var rules = new List<IRateLimiterRule> { testRule.Object, testRule1.Object, testRule2.Object };

            var mockRequestStrategyLogger = new Mock<ILogger<RequestStrategy>>();
            var request = new Mock<RequestStrategy>(mockRequestStrategyLogger.Object);




           
            
            var validator = new RequestLimitValidator(_logger, rules);

            //validator.Validate(request);
            //validator.Validate(request);
        }
    }
}
