   using System;
   using RateLimiter.Contracts;
   using RateLimiter.Infrastructure;
   using RateLimiter.Rules;
   using Microsoft.Extensions.Logging;
   using Moq;
   using System.Collections.Generic;
   using System.Threading.Tasks;
   using Xunit;

   namespace RateLimiter.Tests
   {
       public class DefaultEngineTests
       {
           private readonly Mock<ILogger<DefaultEngine>> _loggerMock;
           private readonly DefaultEngine _engine;

           public DefaultEngineTests()
           {
               _loggerMock = new Mock<ILogger<DefaultEngine>>();
               _engine = new DefaultEngine(_loggerMock.Object);
           }

           [Fact]
           public async Task ProcessRules_AllRulesPass_ReturnsAllowRequestResult()
           {
               // Arrange
               var contextMock = new List<RequestDetails> { new RequestDetails() };
               var ruleMock = new Mock<IRateRule>();
               ruleMock.Setup(r => r.Evaluate(It.IsAny<IEnumerable<RequestDetails>>()))
                       .ReturnsAsync(new AllowRequestResult(true, null));
               var rules = new List<IRateRule> { ruleMock.Object };

               // Act
               var result = await _engine.ProcessRules(rules, contextMock);

               // Assert
               Assert.True(result.AllowRequest);
               Assert.Null(result.Reason);
           }

           [Fact]
           public async Task ProcessRules_OneRuleFails_ReturnsDenyRequestResult()
           {
               // Arrange
               var contextMock = new List<RequestDetails> { new RequestDetails() };
               var ruleMock1 = new Mock<IRateRule>();
               ruleMock1.Setup(r => r.Evaluate(It.IsAny<IEnumerable<RequestDetails>>()))
                        .ReturnsAsync(new AllowRequestResult(true, null));
               var ruleMock2 = new Mock<IRateRule>();
               ruleMock2.Setup(r => r.Evaluate(It.IsAny<IEnumerable<RequestDetails>>()))
                        .ReturnsAsync(new AllowRequestResult(false, "Rule failed"));
               var rules = new List<IRateRule> { ruleMock1.Object, ruleMock2.Object };

               // Act
               var result = await _engine.ProcessRules(rules, contextMock);

               // Assert
               Assert.False(result.AllowRequest);
               Assert.Equal("Rule failed", result.Reason);
           }

           [Fact]
           public async Task ProcessRules_WithNullRules_ThrowsArgumentNullException()
           {
               // Arrange
               var contextMock = new List<RequestDetails> { new RequestDetails() };

               // Act & Assert
               await Assert.ThrowsAsync<ArgumentNullException>(() => _engine.ProcessRules(null, contextMock));
           }

           [Fact]
           public async Task ProcessRules_WithNullContext_ThrowsArgumentNullException()
           {
               // Arrange
               var rulesMock = new List<IRateRule> { new Mock<IRateRule>().Object };

               // Act & Assert
               await Assert.ThrowsAsync<ArgumentNullException>(() => _engine.ProcessRules(rulesMock, null));
           }
       }
   }