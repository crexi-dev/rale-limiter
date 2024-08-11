using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Rule.Request.LastCall;
using RateLimiter.Rule.RequestPerTimeSpan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
