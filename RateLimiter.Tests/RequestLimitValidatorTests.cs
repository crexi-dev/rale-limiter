using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Interface;
using RateLimiter.Model;
using System.Collections.Generic;
using System.Linq;

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
        public void Validator_Return_True()
        {
            var ruleOne = new Mock<IRateLimiterRule>();
            ruleOne.Setup( x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);
            var ruleTwo = new Mock<IRateLimiterRule>();
            ruleTwo.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);

            var rules = new List<IRateLimiterRule>
            {
                ruleOne.Object,
                ruleTwo.Object
            };

            var regionRuleService = new Mock<IRateLimiterRegionRuleService>();
            regionRuleService.Setup(x => x.GetRulesByRegion(It.IsAny<string>())).Returns(rules);

            var validator = new RequestLimitValidator(_logger, regionRuleService.Object);

            var request = new Request();
            request.Region = "US";
            Assert.True(validator.Validate(request));


        }
        [Test]
        public void Validator_Return_Mix_True_False()
        {
            var ruleOne = new Mock<IRateLimiterRule>();
            ruleOne.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(false);
            var ruleTwo = new Mock<IRateLimiterRule>();
            ruleTwo.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);

            var rules = new List<IRateLimiterRule>
            {
                ruleOne.Object,
                ruleTwo.Object
            };

            var regionRuleService = new Mock<IRateLimiterRegionRuleService>();
            regionRuleService.Setup(x => x.GetRulesByRegion(It.IsAny<string>())).Returns(rules);

            var validator = new RequestLimitValidator(_logger, regionRuleService.Object);

            var request = new Request();
            request.Region = "US";
            Assert.False(validator.Validate(request));
        }
        [Test]
        public void Validator_Return_False_()
        {
            var ruleOne = new Mock<IRateLimiterRule>();
            ruleOne.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(false);
            var ruleTwo = new Mock<IRateLimiterRule>();
            ruleTwo.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(false);

            var rules = new List<IRateLimiterRule>
            {
                ruleOne.Object,
                ruleTwo.Object
            };

            var regionRuleService = new Mock<IRateLimiterRegionRuleService>();
            regionRuleService.Setup(x => x.GetRulesByRegion(It.IsAny<string>())).Returns(rules);

            var validator = new RequestLimitValidator(_logger, regionRuleService.Object);

            var request = new Request();
            request.Region = "US";
            Assert.False(validator.Validate(request));
        }
    }
}
