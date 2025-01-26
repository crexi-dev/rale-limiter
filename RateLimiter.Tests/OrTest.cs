using Moq;
using NUnit.Framework;
using RateLimiter.Interfaces;
using RateLimiter.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Tests
{
    [TestFixture]
    public class OrTest
    {
        private Mock<IRule> _rule1Mock;
        private Mock<IRule> _rule2Mock;
        private Or _orRule;

        [SetUp]
        public void SetUp()
        {
            _rule1Mock = new Mock<IRule>();
            _rule2Mock = new Mock<IRule>();
        }

        [Test]
        public void Check_AllRulesReturnFalse_ReturnsFalse()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();

            _rule1Mock.Setup(r => r.Check(identifier)).Returns(false);
            _rule2Mock.Setup(r => r.Check(identifier)).Returns(false);

            _orRule = new Or(new[] { _rule1Mock.Object, _rule2Mock.Object });

            // Act
            var result = _orRule.Check(identifier);

            // Assert
            Assert.IsFalse(result);
            _rule1Mock.Verify(r => r.Check(identifier), Times.Once);
            _rule2Mock.Verify(r => r.Check(identifier), Times.Once);
        }

        [Test]
        public void Check_OneRuleReturnsTrue_ReturnsTrue()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();

            _rule1Mock.Setup(r => r.Check(identifier)).Returns(false);
            _rule2Mock.Setup(r => r.Check(identifier)).Returns(true);

            _orRule = new Or(new[] { _rule1Mock.Object, _rule2Mock.Object });

            // Act
            var result = _orRule.Check(identifier);

            // Assert
            Assert.IsTrue(result);
            _rule1Mock.Verify(r => r.Check(identifier), Times.Once);
            _rule2Mock.Verify(r => r.Check(identifier), Times.Once);
        }

        [Test]
        public void Check_FirstRuleReturnsTrue_ReturnsTrueAndShortCircuits()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();

            _rule1Mock.Setup(r => r.Check(identifier)).Returns(true);
            _rule2Mock.Setup(r => r.Check(identifier)).Returns(false);

            _orRule = new Or(new[] { _rule1Mock.Object, _rule2Mock.Object });

            // Act
            var result = _orRule.Check(identifier);

            // Assert
            Assert.IsTrue(result);
            _rule1Mock.Verify(r => r.Check(identifier), Times.Once);
            _rule2Mock.Verify(r => r.Check(identifier), Times.Never); // Short-circuits
        }

        [Test]
        public void Check_NoRules_ReturnsFalse()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();

            _orRule = new Or(Array.Empty<IRule>());

            // Act
            var result = _orRule.Check(identifier);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
