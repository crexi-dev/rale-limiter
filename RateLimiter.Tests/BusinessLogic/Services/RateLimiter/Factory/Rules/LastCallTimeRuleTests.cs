using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using RateLimiter.BusinessLogic.Models;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.EU;
using RateLimiter.Core.Domain.Entity;
using RateLimiter.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiter.Tests.BusinessLogic.Services.RateLimiter.Factory.Rules
{
	public class LastCallTimeRuleTests
	{
		private readonly IFixture _fixture;
		private readonly Mock<IRequestRepository> _requestRepositoryMock;
		private readonly LastCallTimeRule _rule;

		public LastCallTimeRuleTests()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
			_requestRepositoryMock = _fixture.Freeze<Mock<IRequestRepository>>();
			_rule = new LastCallTimeRule(_requestRepositoryMock.Object);
		}

		[Fact]
		public async Task ApplyToRequest_WhenNoPreviousRequests_ShouldCallAddRequestAsync()
		{
			// Arrange
			var requestDto = _fixture.Create<RequestDto>();
			_requestRepositoryMock.Setup(r => r.GetRequestsAsync(requestDto.Id)).ReturnsAsync(new List<Request>());

			// Act
			var result = await _rule.ApplyToRequest(requestDto);

			// Assert
			Assert.True(result);
			_requestRepositoryMock.Verify(r => r.AddRequestAsync(It.IsAny<Request>()), Times.Once);
		}

		[Fact]
		public async Task ApplyToRequest_WhenTimeSinceLastRequestIsGreaterThanInterval_ShouldCallAddRequestAsync()
		{
			// Arrange
			var requestDto = _fixture.Create<RequestDto>();
			var previousRequests = new List<Request>
			{
				_fixture
					.Build<Request>()
					.With(x => x.DateTime, DateTime.UtcNow.AddMinutes(-3))
					.Create(),
			};

			_requestRepositoryMock.Setup(r => r.GetRequestsAsync(requestDto.Id)).ReturnsAsync(previousRequests);

			// Act
			var result = await _rule.ApplyToRequest(requestDto);

			// Assert
			Assert.True(result);
			_requestRepositoryMock.Verify(r => r.AddRequestAsync(It.IsAny<Request>()), Times.Once);
		}

		[Fact]
		public async Task ApplyToRequest_WhenTimeSinceLastRequestIsLessThanInterval_ShouldNotCallAddRequestAsync()
		{
			// Arrange
			var requestDto = _fixture.Create<RequestDto>();
			var previousRequests = new List<Request>
			{
				_fixture
					.Build<Request>()
					.With(x => x.DateTime, DateTime.UtcNow.AddMinutes(-1))
					.Create(),
			};

			_requestRepositoryMock.Setup(r => r.GetRequestsAsync(requestDto.Id)).ReturnsAsync(previousRequests);

			// Act
			var result = await _rule.ApplyToRequest(requestDto);

			// Assert
			Assert.False(result);
			_requestRepositoryMock.Verify(repository => repository.AddRequestAsync(It.IsAny<Request>()), Times.Never);
		}
	}
}
