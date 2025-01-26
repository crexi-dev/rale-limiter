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
    public class AndTest
    {
        private Mock<IRule> _rule1Mock;
        private Mock<IRule> _rule2Mock;
        private And _andRule;

        [SetUp]
        public void SetUp()
        {
            _rule1Mock = new Mock<IRule>();
            _rule2Mock = new Mock<IRule>();
        }

        [Test]
        public void Check_AllRulesReturnTrue_ReturnsTrue()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();

            _rule1Mock.Setup(r => r.Check(identifier)).Returns(true);
            _rule2Mock.Setup(r => r.Check(identifier)).Returns(true);

            _andRule = new And(new[] { _rule1Mock.Object, _rule2Mock.Object });

            // Act
            var result = _andRule.Check(identifier);

            // Assert
            Assert.IsTrue(result);
            _rule1Mock.Verify(r => r.Check(identifier), Times.Once);
            _rule2Mock.Verify(r => r.Check(identifier), Times.Once);
        }

        [Test]
        public void Check_OneRuleReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();

            _rule1Mock.Setup(r => r.Check(identifier)).Returns(true);
            _rule2Mock.Setup(r => r.Check(identifier)).Returns(false);

            _andRule = new And(new[] { _rule1Mock.Object, _rule2Mock.Object });

            // Act
            var result = _andRule.Check(identifier);

            // Assert
            Assert.IsFalse(result);
            _rule1Mock.Verify(r => r.Check(identifier), Times.Once);
            _rule2Mock.Verify(r => r.Check(identifier), Times.Once);
        }

        [Test]
        public void Check_AllRulesReturnFalse_ReturnsFalse()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();

            _rule1Mock.Setup(r => r.Check(identifier)).Returns(false);
            _rule2Mock.Setup(r => r.Check(identifier)).Returns(false);

            _andRule = new And(new[] { _rule1Mock.Object, _rule2Mock.Object });

            // Act
            var result = _andRule.Check(identifier);

            // Assert
            Assert.IsFalse(result);
            _rule1Mock.Verify(r => r.Check(identifier), Times.Once);
            _rule2Mock.Verify(r => r.Check(identifier), Times.Never); // Short-circuit: stops at the first false
        }

        [Test]
        public void Check_NoRules_ReturnsTrue()
        {
            // Arrange
            var identifier = Mock.Of<IIdentifier>();

            _andRule = new And(Array.Empty<IRule>());

            // Act
            var result = _andRule.Check(identifier);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
