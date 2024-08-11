using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Model;
using RateLimiter.Rule.Request.LastCall;
using System;
using System.Collections.Generic;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimiterLastCallTest
    {
        private ILogger<LastCallValidator> _logger;
        public RateLimiterLastCallTest() 
        {
            _logger = new Mock<ILogger<LastCallValidator>>().Object;
        }
        [Test]
        public void Last_Request_Call_Pass_TimePeriod_Current_Access_Set_To_Min_Should_Return_True()
        {           
            var testValidator = new LastCallValidator(5, _logger, new List<string> { "US" });
            var request = new Request();
            request.AccessTime.Add(DateTime.Now);
            Assert.False(testValidator.VerifyAccess(request));
        }

        [Test]
        public void Last_Request_Call_Pass_TimePeriod_Current_Time_Greater_New_Return_False()
        {
            var testValidator = new LastCallValidator(5, _logger, new List<string> { "US" });

            var request = new Request();
            request.CurrentTime = DateTime.Now.AddMinutes(5);
            request.AccessTime.Add(DateTime.Now.AddMinutes(5));
            Assert.False(testValidator.VerifyAccess(request));
        }

        [Test]
        public void Last_Request_Call_Pass_TimePeriod_New_Request_Should_True()
        {
            var testValidator = new LastCallValidator(5, _logger, new List<string> { "US" });

            var request = new Request();
            request.CurrentTime = DateTime.Now.AddMinutes(5);            
            Assert.False(testValidator.VerifyAccess(request));
        }
    }
}
