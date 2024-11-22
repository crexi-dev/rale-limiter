using NUnit.Framework;
using RateLimiter.Base;
using RateLimiter.Config;
using System.Collections.Generic;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class LinkedLimiterTests
    {
        private LimiterConfig _tokenLimiterConfig;
        private LimiterConfig _fixedWindowLimiterConfig;

        [SetUp]
        public void SetUp()
        {
            _tokenLimiterConfig = new LimiterConfig
            {
                LimiterType = LimiterType.TokenLimiter,
                MaxTokens = 10
            };

            _fixedWindowLimiterConfig = new LimiterConfig
            {
                LimiterType = LimiterType.FixedWindowLimiter,
                MaxTokens = 10,
                MaxTimeInSeconds = 60
            };
        }

        [Test]
        public void AcquireLease_ShouldAcquireLease_WhenSingleTokenLimiterHasTokens()
        {
            // Arrange
            var tokenLimiter = new TokenLimiter(_tokenLimiterConfig);
            var linkedLimiter = new LinkedLimiter(new List<BaseRateLimiter> { tokenLimiter }, _tokenLimiterConfig);
            var leaseConfig = new LeaseConfig { ResourceName = "testResource", Tokens = 1 };

            // Act
            var lease = linkedLimiter.AcquireLease(leaseConfig);

            // Assert
            Assert.IsTrue(lease.IsAcquired);
            Assert.AreEqual(9, tokenLimiter.AvailableTokens);
        }

        [Test]
        public void AcquireLease_ShouldNotAcquireLease_WhenSingleTokenLimiterHasNoTokens()
        {
            // Arrange
            var tokenLimiter = new TokenLimiter(_tokenLimiterConfig);
            tokenLimiter.AcquireLease(new LeaseConfig { ResourceName = "testResource", Tokens = 10 });
            var linkedLimiter = new LinkedLimiter(new List<BaseRateLimiter> { tokenLimiter }, _tokenLimiterConfig);
            var leaseConfig = new LeaseConfig { ResourceName = "testResource", Tokens = 1 };

            // Act
            var lease = linkedLimiter.AcquireLease(leaseConfig);

            // Assert
            Assert.IsFalse(lease.IsAcquired);
            Assert.AreEqual(0, tokenLimiter.AvailableTokens);
        }

        [Test]
        public void AcquireLease_ShouldAcquireLease_WhenTwoTokenLimitersHaveTokens()
        {
            // Arrange
            var tokenLimiter1 = new TokenLimiter(_tokenLimiterConfig);
            var tokenLimiter2 = new TokenLimiter(_tokenLimiterConfig);
            var linkedLimiter = new LinkedLimiter(new List<BaseRateLimiter> { tokenLimiter1, tokenLimiter2 }, _tokenLimiterConfig);
            var leaseConfig = new LeaseConfig { ResourceName = "testResource", Tokens = 1 };

            // Act
            var lease = linkedLimiter.AcquireLease(leaseConfig);

            // Assert
            Assert.IsTrue(lease.IsAcquired);
            Assert.AreEqual(9, tokenLimiter1.AvailableTokens);
            Assert.AreEqual(9, tokenLimiter2.AvailableTokens);
        }

        [Test]
        public void AcquireLease_ShouldNotAcquireLease_WhenOneOfTwoTokenLimitersHasNoTokens()
        {
            // Arrange
            var tokenLimiter1 = new TokenLimiter(_tokenLimiterConfig);
            var tokenLimiter2 = new TokenLimiter(_tokenLimiterConfig);
            tokenLimiter1.AcquireLease(new LeaseConfig { ResourceName = "testResource", Tokens = 10 });
            var linkedLimiter = new LinkedLimiter(new List<BaseRateLimiter> { tokenLimiter1, tokenLimiter2 }, _tokenLimiterConfig);
            var leaseConfig = new LeaseConfig { ResourceName = "testResource", Tokens = 1 };

            // Act
            var lease = linkedLimiter.AcquireLease(leaseConfig);

            // Assert
            Assert.IsFalse(lease.IsAcquired);
            Assert.AreEqual(0, tokenLimiter1.AvailableTokens);
            Assert.AreEqual(10, tokenLimiter2.AvailableTokens);
        }

        [Test]
        public void AcquireLease_ShouldAcquireLease_WhenTokenLimiterAndFixedWindowLimiterHaveTokens()
        {
            // Arrange
            var tokenLimiter = new TokenLimiter(_tokenLimiterConfig);
            var fixedWindowLimiter = new FixedWindowLimiter(_fixedWindowLimiterConfig);
            var linkedLimiter = new LinkedLimiter(new List<BaseRateLimiter> { tokenLimiter, fixedWindowLimiter }, _tokenLimiterConfig);
            var leaseConfig = new LeaseConfig { ResourceName = "testResource", Tokens = 1 };

            // Act
            var lease = linkedLimiter.AcquireLease(leaseConfig);

            // Assert
            Assert.IsTrue(lease.IsAcquired);
            Assert.AreEqual(9, tokenLimiter.AvailableTokens);
            Assert.AreEqual(1, fixedWindowLimiter.CurrentTokens);
        }

        [Test]
        public void AcquireLease_ShouldNotAcquireLease_WhenTokenLimiterHasNoTokensAndFixedWindowLimiterHasTokens()
        {
            // Arrange
            var tokenLimiter = new TokenLimiter(_tokenLimiterConfig);
            var fixedWindowLimiter = new FixedWindowLimiter(_fixedWindowLimiterConfig);
            tokenLimiter.AcquireLease(new LeaseConfig { ResourceName = "testResource", Tokens = 10 });
            var linkedLimiter = new LinkedLimiter(new List<BaseRateLimiter> { tokenLimiter, fixedWindowLimiter }, _tokenLimiterConfig);
            var leaseConfig = new LeaseConfig { ResourceName = "testResource", Tokens = 1 };

            // Act
            var lease = linkedLimiter.AcquireLease(leaseConfig);

            // Assert
            Assert.IsFalse(lease.IsAcquired);
            Assert.AreEqual(0, tokenLimiter.AvailableTokens);
            Assert.AreEqual(0, fixedWindowLimiter.CurrentTokens);
        }

        [Test]
        public void ReleaseLease_ShouldReleaseLease_WhenLeaseIsRelinquished()
        {
            // Arrange
            var tokenLimiter = new TokenLimiter(_tokenLimiterConfig);
            var linkedLimiter = new LinkedLimiter(new List<BaseRateLimiter> { tokenLimiter }, _tokenLimiterConfig);
            var leaseConfig = new LeaseConfig { ResourceName = "testResource", Tokens = 1 };
            var lease = linkedLimiter.AcquireLease(leaseConfig);

            // Act
            lease.RelinquishLease();

            // Assert
            Assert.IsFalse(lease.IsAcquired);
            Assert.AreEqual(10, tokenLimiter.AvailableTokens);
        }
    }
}

