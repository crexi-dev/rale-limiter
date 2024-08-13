//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;
//using RateLimiter.Interface;
//using RateLimiter.Interface.Rule;
//using System.Collections.Generic;
//using System.Linq;

//namespace RateLimiter.Tests
//{
//    [TestFixture]
//    public class RequestLimitValidatorTests
//    {
//        private ILogger<RequestLimitValidator> _logger;
//        public RequestLimitValidatorTests()
//        {
//            _logger = new Mock<ILogger<RequestLimitValidator>>().Object;
//        }

//        [Test]
//        public void RequestLimitValidator_Region_Set_Corret_Number()
//        {
//            var testRule = new Mock<IRateLimiterRule>();
//            testRule.Setup(x => x.SupportedRegion).Returns(new List<string> { "US" });

//            var testRule1 = new Mock<IRateLimiterRule>();
//            testRule1.Setup(x => x.SupportedRegion).Returns(new List<string> { "US" });

//            var rules = new List<IRateLimiterRule> { testRule.Object, testRule1.Object };

//            var requestStrategy = new RequestStrategy();
//            requestStrategy.Region = "US";

//            var regionService = new Mock<IRateLimiterRegionRuleService>();
//            regionService.Setup(x => x.GetRulesByRegion(It.IsAny<string>())).Returns(rules);    

//            var validator = new RequestLimitValidator(_logger, regionService.Object);
//            var result = validator.Validate(requestStrategy);
//            Assert.IsTrue(requestStrategy.Rules != null);
//            Assert.IsTrue(requestStrategy.Rules.Count() == 2);
//        }

//        [Test]
//        public void RequestLimitValidator_Region_Set_Corret_Number_EmptyString()
//        {
//            var testRule = new Mock<IRateLimiterRule>();
//            testRule.Setup(x => x.SupportedRegion).Returns(new List<string> { "US" });

//            var testRule1 = new Mock<IRateLimiterRule>();
//            testRule1.Setup(x => x.SupportedRegion).Returns(new List<string> { });

//            var rules = new List<IRateLimiterRule> { testRule.Object, testRule1.Object };
                        
//            var requestStrategy = new RequestStrategy();
//            requestStrategy.Region = "US";

//            var regionService = new Mock<IRateLimiterRegionRuleService>();
//            regionService.Setup(x => x.GetRulesByRegion(It.IsAny<string>())).Returns(rules);

//            var validator = new RequestLimitValidator(_logger, regionService.Object);
//            validator.Validate(requestStrategy);
//            Assert.IsTrue(requestStrategy.Rules != null);
//            Assert.IsTrue(requestStrategy.Rules.Count() == 1);

//        }

//        [Test]
//        public void RequestLimitValidator_Region_Set_Corret_Number_More_Than_1_Region()
//        {
//            var testRule = new Mock<IRateLimiterRule>();
//            testRule.Setup(x => x.SupportedRegion).Returns(new List<string> { "US", "EU" });

//            var testRule1 = new Mock<IRateLimiterRule>();
//            testRule1.Setup(x => x.SupportedRegion).Returns(new List<string> { });

//            var rules = new List<IRateLimiterRule> { testRule.Object, testRule1.Object };
//            var requestStrategy = new RequestStrategy();
//            requestStrategy.Region = "US";

//            var regionService = new Mock<IRateLimiterRegionRuleService>();
//            regionService.Setup(x => x.GetRulesByRegion(It.IsAny<string>())).Returns(rules);


//            var validator = new RequestLimitValidator(_logger, regionService.Object);
//            validator.Validate(requestStrategy);
//            Assert.IsTrue(requestStrategy.Rules != null);
//            Assert.IsTrue(requestStrategy.Rules.Count() == 1);

//        }
//        [Test]
//        public void RequestLimitValidator_Region_Set_Corret_Empty_Region()
//        {
//            var testRule = new Mock<IRateLimiterRule>();
//            testRule.Setup(x => x.SupportedRegion).Returns(new List<string> { "US", "EU" });

//            var testRule1 = new Mock<IRateLimiterRule>();
//            testRule1.Setup(x => x.SupportedRegion).Returns(new List<string> { });

//            var rules = new List<IRateLimiterRule> { testRule.Object, testRule1.Object };          
//            var requestStrategy = new RequestStrategy();
//            requestStrategy.Region = "US";

//            var regionService = new Mock<IRateLimiterRegionRuleService>();
//            regionService.Setup(x => x.GetRulesByRegion(It.IsAny<string>())).Returns(rules);

//            var validator = new RequestLimitValidator(_logger, regionService.Object);
//            validator.Validate(requestStrategy);
//            Assert.IsTrue(requestStrategy.Rules != null);
//            Assert.IsTrue(requestStrategy.Rules.Count() == 2);
//        }
//    }
//}
