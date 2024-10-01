using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RateLimiter.Rules;

namespace RateLimiter.Tests.Rules
{
    [TestFixture]
    public class RuleServiceTest
    {
        private Mock<IRule> ruleOne;
        private Mock<IRule> ruleTwo;
        private RuleService sut;

        [SetUp]
        public void Init()
        {
            this.ruleOne = new Mock<IRule>();
            this.ruleTwo = new Mock<IRule>();

            this.ruleOne.Setup(r => r.Name).Returns("ruleOne");
            this.ruleTwo.Setup(r => r.Name).Returns("ruleTwo");

            this.sut = new RuleService();

            this.sut.AddResourceRules(
                new IRule[] { this.ruleOne.Object, this.ruleTwo.Object },
                new Dictionary<string, IEnumerable<string>>()
                {
                    ["testOne"] = new string[] { "ruleOne" },
                    ["testTwo"] = new string[] { "ruleTwo" },
                    ["testThree"] = new string[] { "ruleOne", "ruleTwo" },
                });
        }

        [Test]
        public async Task AllowBlockTestOne([Values(true, false)] bool desiredResult)
        {
            this.ruleOne.Setup(r => r.Allow(It.IsAny<Client>())).Returns(Task.FromResult(desiredResult));

            var allow = await this.sut.Allow("testOne", new Client("testId", "US"));

            Assert.That(allow, Is.EqualTo(desiredResult));
        }

        [Test]
        public async Task AllowBlockTestTwo([Values(true, false)] bool desiredResult)
        {
            this.ruleTwo.Setup(r => r.Allow(It.IsAny<Client>())).Returns(Task.FromResult(desiredResult));

            var allow = await this.sut.Allow("testTwo", new Client("testId", "US"));

            Assert.That(allow, Is.EqualTo(desiredResult));
        }

        [Test]
        public async Task AllowBlockTestThree([Values(true, false)] bool desiredResult)
        {
            this.ruleOne.Setup(r => r.Allow(It.IsAny<Client>())).Returns(Task.FromResult(desiredResult));
            this.ruleTwo.Setup(r => r.Allow(It.IsAny<Client>())).Returns(Task.FromResult(desiredResult));

            var allow = await this.sut.Allow("testThree", new Client("testId", "US"));

            Assert.That(allow, Is.EqualTo(desiredResult));
        }

        [Test]
        public async Task RuleSplitTest()
        {
            this.ruleOne.Setup(r => r.Allow(It.IsAny<Client>())).Returns(Task.FromResult(true));
            this.ruleTwo.Setup(r => r.Allow(It.IsAny<Client>())).Returns(Task.FromResult(false));

            var allow = await this.sut.Allow("testThree", new Client("testId", "US"));

            Assert.That(allow, Is.EqualTo(false));
        }
    }
}
