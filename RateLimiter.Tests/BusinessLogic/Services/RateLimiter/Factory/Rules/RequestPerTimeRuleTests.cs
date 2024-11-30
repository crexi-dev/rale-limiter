using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using RateLimiter.BusinessLogic.Models;
using RateLimiter.BusinessLogic.Services.Implementation.RateLimiter.Rules.USA;
using RateLimiter.Core.Domain.Entity;
using RateLimiter.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiter.Tests.BusinessLogic.Services.RateLimiter.Factory.Rules
{
	public class RequestPerTimeRuleTests
	{
		private readonly IFixture _fixture;
		private readonly Mock<IRequestRepository> _requestRepositoryMock;
		private readonly RequestPerTimeRule _rule;

		public RequestPerTimeRuleTests()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
			_requestRepositoryMock = _fixture.Freeze<Mock<IRequestRepository>>();
			_rule = new RequestPerTimeRule(_requestRepositoryMock.Object);
		}

		[Theory]
		[MemberData(nameof(DataForWhenRequestsWithinTimeLimit))]
		public async Task ApplyToRequest_WhenRequestCountDoesNotExceedLimit_ShouldCallAddRequestAsync(List<Request> previousRequests)
		{
			// Arrange
			var requestDto = _fixture.Create<RequestDto>();
			_requestRepositoryMock
				.Setup(r => r.CleanupOldRequestsAsync(It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
				.Returns(Task.CompletedTask);
			_requestRepositoryMock.Setup(r => r.GetRequestsAsync(requestDto.Id)).ReturnsAsync(previousRequests);

			// Act
			var result = await _rule.ApplyToRequest(requestDto);

			// Assert
			Assert.True(result);
			_requestRepositoryMock.Verify(r => r.AddRequestAsync(It.IsAny<Request>()), Times.Once);
		}

		[Fact]
		public async Task ApplyToRequest_WhenRequestCountExceedsLimit_ShouldNotCallAddRequestAsync()
		{
			// Arrange
			var requestDto = _fixture.Create<RequestDto>();
			var previousRequests = _fixture
				.Build<Request>()
				.CreateMany(3)
				.ToList();

			_requestRepositoryMock
				.Setup(r => r.CleanupOldRequestsAsync(It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
				.Returns(Task.CompletedTask);
			_requestRepositoryMock.Setup(r => r.GetRequestsAsync(requestDto.Id)).ReturnsAsync(previousRequests);

			// Act
			var result = await _rule.ApplyToRequest(requestDto);

			// Assert
			Assert.False(result);
			_requestRepositoryMock.Verify(r => r.AddRequestAsync(It.IsAny<Request>()), Times.Never);
		}

		public static IEnumerable<object[]> DataForWhenRequestsWithinTimeLimit()
			=> new List<object[]>
			{
					new object[]
					{
						new List<Request>(),

					},
					new object[]
					{
						new Fixture()
							.Build<Request>()
							.CreateMany(1)
							.ToList()
					},
					new object[]
					{
						new Fixture()
							.Build<Request>()
							.CreateMany(2)
							.ToList()
					},
			};
	}
}
