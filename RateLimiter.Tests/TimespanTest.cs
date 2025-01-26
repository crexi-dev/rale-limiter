using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Rules;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class TimespanTest
    {
        private Mock<ITimespanHistory> _timespanHistoryMock;
        private Timespan _timespan;
        private const uint TimespanValue = 10; // 10 seconds

        [SetUp]
        public void SetUp()
        {
            _timespanHistoryMock = new Mock<ITimespanHistory>();
            _timespan = new Timespan(_timespanHistoryMock.Object, TimespanValue);
        }

        [Test]
        public void Check_LastRequestOutsideTimespan_ReturnsTrue()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();
            var now = DateTime.Now;
            var lastRequestTime = now.AddSeconds(-20); // Last request was 20 seconds ago

            _timespanHistoryMock
                .Setup(h => h.GetLastRequestDate(identifier))
                .Returns(lastRequestTime);

            // Act
            var result = _timespan.Check(identifier);

            // Assert
            Assert.IsTrue(result);
            //_timespanHistoryMock.Verify(h => h.Record(identifier, It.Is<DateTime>(dt => dt == now)), Times.Once);
        }

        [Test]
        public void Check_LastRequestWithinTimespan_ReturnsFalse()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();
            var now = DateTime.Now;
            var lastRequestTime = now.AddSeconds(-5); // Last request was 5 seconds ago

            _timespanHistoryMock
                .Setup(h => h.GetLastRequestDate(identifier))
                .Returns(lastRequestTime);

            // Act
            var result = _timespan.Check(identifier);

            // Assert
            Assert.IsFalse(result);
            //_timespanHistoryMock.Verify(h => h.Record(It.IsAny<IIdentifier>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public void Check_NoPreviousRequests_ReturnsTrue()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();
            var now = DateTime.Now;

            _timespanHistoryMock
                .Setup(h => h.GetLastRequestDate(identifier))
                .Returns(DateTime.MinValue); // Simulate no previous request

            // Act
            var result = _timespan.Check(identifier);

            // Assert
            Assert.IsTrue(result);
            //_timespanHistoryMock.Verify(h => h.Record(identifier, It.Is<DateTime>(dt => dt == now)), Times.Once);
        }
    }
}
