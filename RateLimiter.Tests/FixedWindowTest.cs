using NUnit.Framework;
using System;
using Moq;
using NUnit.Framework;
using RateLimiter.Rules;
using RateLimiter.Interfaces;


namespace RateLimiter.Tests
{
    [TestFixture]
    public class FixedWindowTest
    {
        private Mock<IFixedWindowHistory> _fixedWindowHistoryMock;
        private FixedWindow _fixedWindow;
        private const uint MaxCount = 5;
        private const uint Window = 10; // 10 seconds

        [SetUp]
        public void SetUp()
        {
            _fixedWindowHistoryMock = new Mock<IFixedWindowHistory>();
            _fixedWindow = new FixedWindow(_fixedWindowHistoryMock.Object, MaxCount, Window);
        }

        [Test]
        public void Check_RequestWithinLimit_ReturnsTrue()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();
            var now = DateTime.Now;

            _fixedWindowHistoryMock
                .Setup(h => h.GetRequestCount(identifier, It.Is<DateTime>(d => d <= DateTime.Now),
                It.Is<DateTime>(d => d >= DateTime.Now.AddSeconds(-Window))))
                .Returns(4); // Simulate 4 requests within the window

            // Act
            var result = _fixedWindow.Check(identifier);

            // Assert
            Assert.IsTrue(result);
            _fixedWindowHistoryMock.Verify(h => h.Record(identifier, It.Is<DateTime>(dt => Math.Abs((dt - now).TotalMilliseconds) < 100)), Times.Once);
        }

        [Test]
        public void Check_RequestExceedsLimit_ReturnsFalse()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();
            var now = DateTime.Now;

            _fixedWindowHistoryMock
                .Setup(h => h.GetRequestCount(identifier, It.Is<DateTime>(d => d <= DateTime.Now),
                It.Is<DateTime>(d => d >= DateTime.Now.AddSeconds(-Window))))
                .Returns(6); // Simulate 6 requests within the window

            // Act
            var result = _fixedWindow.Check(identifier);

            // Assert
            Assert.IsFalse(result);
            _fixedWindowHistoryMock.Verify(h => h.Record(It.IsAny<IIdentifier>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public void Check_RequestAtLimit_ReturnsTrue()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();
            var now = DateTime.Now;

            _fixedWindowHistoryMock
                .Setup(h => h.GetRequestCount(identifier, It.Is<DateTime>(d => d <= DateTime.Now),
                It.Is<DateTime>(d => d >= DateTime.Now.AddSeconds(-Window))))
                .Returns(5); // Simulate 5 requests, which is at the limit

            // Act
            var result = _fixedWindow.Check(identifier);

            // Assert
            Assert.IsTrue(result);
            _fixedWindowHistoryMock.Verify(h => h.Record(identifier, It.Is<DateTime>(dt => Math.Abs((dt - now).TotalMilliseconds) < 100)), Times.Once);
        }
    }
}
