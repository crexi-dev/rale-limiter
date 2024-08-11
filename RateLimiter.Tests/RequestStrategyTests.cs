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
    public class RequestStrategyTests
    {
        private readonly ILogger<RequestStrategy> _logger;

        public RequestStrategyTests()
        { 
            _logger = new Mock<ILogger<RequestStrategy>>().Object;
        }
        [Test]
        public void RequestStrategy_Rules_Not_Set_Return_False()
        {
            var strategy = new RequestStrategy(_logger);
            var result = strategy.VerifyAccess();
            Assert.False(result);
        }

        [Test]
        public void RequestStrategy_Rules_Set_Return_True()
        {
            var testRule = new Mock<IRateLimiterRule>();
            testRule.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);             

            var strategy = new RequestStrategy(_logger);
            var rules = new List<IRateLimiterRule> { testRule.Object};

            strategy.AccessTime.Add(DateTime.Now);
            strategy.Rules = rules;   
            var result = strategy.VerifyAccess();

            Assert.True(result);
        }

        [Test]
        public void RequestStrategy_Number_Of_Rules_Set_Return_True()
        {
            var testRule = new Mock<IRateLimiterRule>();
            testRule.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);
            
            var testRule1 = new Mock<IRateLimiterRule>();
            testRule1.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);
            
            var testRule2 = new Mock<IRateLimiterRule>();
            testRule2.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);
            
            var strategy = new RequestStrategy(_logger);
            var rules = new List<IRateLimiterRule> { testRule.Object, testRule1.Object, testRule2.Object };

            strategy.AccessTime.Add(DateTime.Now);
            strategy.Rules = rules;
            var result = strategy.VerifyAccess();

            Assert.True(result);
        }
        [Test]
        public void RequestStrategy_Number_Of_Rules_One_Is_False_Set_Return_True()
        {
            var testRule = new Mock<IRateLimiterRule>();
            testRule.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);

            var testRule1 = new Mock<IRateLimiterRule>();
            testRule1.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(false);

            var testRule2 = new Mock<IRateLimiterRule>();
            testRule2.Setup(x => x.VerifyAccess(It.IsAny<Request>())).Returns(true);

            var strategy = new RequestStrategy(_logger);
            var rules = new List<IRateLimiterRule> { testRule.Object, testRule1.Object, testRule2.Object };

            strategy.AccessTime.Add(DateTime.Now);
            strategy.Rules = rules;
            var result = strategy.VerifyAccess();

            Assert.False(result);
        }
    }
}
