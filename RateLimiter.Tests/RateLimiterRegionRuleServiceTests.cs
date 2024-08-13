using Moq;
using NUnit.Framework;
using RateLimiter.Interface;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimiterRegionRuleServiceTest
    {
        [Test]
        public void GetRulesByRegion_Should_Return_Matching_Rules_And_Rules_Apply_To_All_Region()
        {
            // Arrange
            var region = "US";
            var mockRule1 = new Mock<IRateLimiterRule>();
            mockRule1.Setup(x => x.SupportedRegion).Returns(new List<string> { "US", "CA" });

            var mockRule2 = new Mock<IRateLimiterRule>();
            mockRule2.Setup(x => x.SupportedRegion).Returns(new List<string>());

            var mockRule3 = new Mock<IRateLimiterRule>();
            mockRule3.Setup(x => x.SupportedRegion).Returns(new List<string> { "US", "UK" });

            var rules = new List<IRateLimiterRule> { mockRule1.Object, mockRule2.Object, mockRule3.Object };

            var service = new RateLimiterRegionRuleService(rules);
            var result = service.GetRulesByRegion(region);
            Assert.AreEqual(3, result.Count());           
        }        
    }
}