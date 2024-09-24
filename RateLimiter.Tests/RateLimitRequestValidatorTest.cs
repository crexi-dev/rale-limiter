using NUnit.Framework;
using RateLimiter.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimitRequestValidatorTest
    {
        [TestCase(null, null)]
        [TestCase("u1", null)]
        [TestCase(null, "r1")]
        [TestCase("u1", "r1")]
        public void Validator_Tests(string userId, string resourceId)
        {
            var validator = new RateLimitRequestValidator();
            var result = validator.Validate(new Models.Apis.RateLimitRequest()
            {
                UserId = userId,
                ResourceId = resourceId
            });

            if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(resourceId))
            {
                Assert.AreEqual(2, result.Errors.Count);
            } 
            else if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(resourceId))
            {
                Assert.AreEqual(1, result.Errors.Count);
            }
            else
            {
                Assert.AreEqual(0, result.Errors.Count);
            }
        }
    }
}
