using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Model;
using RateLimiter.Rule.RequestPerTimeSpan;
using System;
using System.Collections.Generic;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimiterPreTimeSpanTests
    {
        private ILogger<PerTimeSpanValidator> _logger;
        public RateLimiterPreTimeSpanTests()
        {
            _logger = new Mock<ILogger<PerTimeSpanValidator>>().Object;


        }

        [Test]
        public void Per_Time_Span_Validaotr_Return_True_Count_Is_Within_Limit()
        {
            var testValidator = new PerTimeSpanValidator(1, 1, _logger, new List<string> { "US" });
            var request = new Request();
            request.CurrentTime = DateTime.Now;
            request.AccessTime.Add(DateTime.MinValue);

            Assert.True(testValidator.VerifyAccess(request));
        }

        [Test]
        public void Per_Time_Span_Validaotr_Return_False_Count_Is_Not_Within_Limit()
        {
            var testValidator = new PerTimeSpanValidator(0, 120, _logger, new List<string> { "US" });
            var request = new Request();
            request.CurrentTime = DateTime.Now;
            request.AccessTime.Add(DateTime.Now);
            request.AccessTime.Add(DateTime.Now);

            Assert.False(testValidator.VerifyAccess(request));
        }
    }
}
