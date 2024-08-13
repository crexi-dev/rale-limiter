using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RateLimiter.Interface;
using RateLimiter.Model;
using System;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class RateLimiterServiceTests
    {
        private Mock<ILogger<RateLimiterService>> _logger;
        private Mock<IRateLimiterRepository> _rateLimiterRepositoryMock;
        private Mock<IRequestLimitValidator> _accessValidatorMock;
        private RateLimiterService _rateLimiterService;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<RateLimiterService>>();
           

            _rateLimiterRepositoryMock = new Mock<IRateLimiterRepository>();
            _accessValidatorMock = new Mock<IRequestLimitValidator>();
            
            _rateLimiterService = new RateLimiterService(_logger.Object, _rateLimiterRepositoryMock.Object, _accessValidatorMock.Object);
        }

        [Test]
        public void Validate_Request_Is_Null_ReturnsFalse()
        {            
            RequestDTO requestDTO = null;         
            bool result = _rateLimiterService.Validate(requestDTO);
            Assert.IsFalse(result);            
        }

        [Test]
        public void Validate_ValidRequest_ReturnsAccessValidatorResult()
        {
            // Arrange
            RequestDTO requestDTO = new RequestDTO();
            Request currentRequest = new Request();
            _rateLimiterRepositoryMock.Setup(x => x.Get(requestDTO)).Returns(currentRequest);
            _accessValidatorMock.Setup(x => x.Validate(currentRequest)).Returns(true);

            // Act
            bool result = _rateLimiterService.Validate(requestDTO);

            // Assert
            Assert.IsTrue(result);
            _rateLimiterRepositoryMock.Verify(x => x.Update(currentRequest), Times.Once);
            _accessValidatorMock.Verify(x => x.Validate(currentRequest), Times.Once);
        }

        [Test]
        public void Validate_ExceptionThrown_LogsErrorAndThrows()
        {
            // Arrange
            RequestDTO requestDTO = new RequestDTO();
            Request currentRequest = new Request();
            _rateLimiterRepositoryMock.Setup(x => x.Get(requestDTO)).Returns(currentRequest);
            _accessValidatorMock.Setup(x => x.Validate(currentRequest)).Throws<Exception>();

            // Act & Assert
            Assert.Throws<Exception>(() => _rateLimiterService.Validate(requestDTO));           
        }
    }
}