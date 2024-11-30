using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using RateLimiter.Api.Infrastructure.Attributes;
using RateLimiter.Api.Infrastructure.Filters;
using RateLimiter.BusinessLogic.Models;
using RateLimiter.BusinessLogic.Services.RateLimiter;
using RateLimiter.Core.Domain.Enums;
using RateLimiter.Core.Exceptions;
using RateLimiter.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiter.Tests.Api.Infrastructure.Filters
{
	public class RequestLimitFilterTests
	{
		private readonly IFixture _fixture;
		private readonly Mock<ILimitService> _limitServiceMock;
		private readonly RequestLimitFilter _filter;

		public RequestLimitFilterTests()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
			_limitServiceMock = _fixture.Freeze<Mock<ILimitService>>();
			_filter = new RequestLimitFilter(_limitServiceMock.Object);
		}

		[Fact]
		public async Task OnActionExecutionAsync_WhenLimitIsNotReached_ShouldCallIsRequestReachedLimitMethod()
		{
			// Arrange
			var appliedRuleAttribute = new AppliedRuleAttribute(It.IsAny<RuleType>());
			var context = CreateActionExecutingContext(appliedRuleAttribute);
			ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(
				context,
				new List<IFilterMetadata>(),
				It.IsAny<object>()
			));

			_limitServiceMock
				.Setup(service => service.IsRequestReachedLimit(It.IsAny<List<RuleType>>(), It.IsAny<RequestDto>()))
				.ReturnsAsync(false);

			// Act
			await _filter.OnActionExecutionAsync(context, next);

			// Assert
			_limitServiceMock.Verify(service => service.IsRequestReachedLimit(
				It.IsAny<List<RuleType>>(),
				It.IsAny<RequestDto>()),
				Times.Once);
		}

		[Fact]
		public async Task OnActionExecutionAsync_WhenLimitIsReached_ShouldThrowTooManyRequestsException()
		{
			// Arrange
			var appliedRuleAttribute = new AppliedRuleAttribute(It.IsAny<RuleType>());
			var context = CreateActionExecutingContext(appliedRuleAttribute);
			ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(
				context,
				new List<IFilterMetadata>(),
				It.IsAny<object>()
			));

			_limitServiceMock
				.Setup(service => service.IsRequestReachedLimit(It.IsAny<List<RuleType>>(), It.IsAny<RequestDto>()))
				.ReturnsAsync(true);

			// Act
			Func<Task> act = () => _filter.OnActionExecutionAsync(context, next);

			// Assert
			await Assert.ThrowsAsync<TooManyRequestsException>(act);
		}


		[Theory]
		[MemberData(nameof(AppliedRuleAttributeForFilter))]
		public async Task OnActionExecutionAsync_WhenAppliedRuleAttributeIsNullOrNoRuleTypes_ShouldNotCallIsRequestReachedLimit(AppliedRuleAttribute appliedRuleAttribute)
		{
			// Arrange
			var context = CreateActionExecutingContext(appliedRuleAttribute);
			ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(
				context,
				new List<IFilterMetadata>(),
				It.IsAny<object>()
			));

			// Act
			await _filter.OnActionExecutionAsync(context, next);

			// Assert
			_limitServiceMock.Verify(service =>
				service.IsRequestReachedLimit(It.IsAny<List<RuleType>>(), It.IsAny<RequestDto>()), Times.Never);
		}

		private ActionExecutingContext CreateActionExecutingContext(AppliedRuleAttribute appliedRuleAttribute)
		{
			_fixture.Customize<RegionType>(c => c.FromFactory(() =>
			{
				var values = Enum.GetValues(typeof(RegionType)).Cast<RegionType>().ToList();
				values.Remove(RegionType.None);

				return values[_fixture.Create<int>() % values.Count];  // Random RegionType, except None
			}));

			var claims = new List<Claim>
			{
				new Claim(nameof(Constants.CustomClaimTypes.Region), _fixture.Create<RegionType>().ToString()),
				new Claim(nameof(Constants.CustomClaimTypes.UniqueIdentifier), Guid.NewGuid().ToString())
			};

			var httpContext = _fixture
				.Build<DefaultHttpContext>()
				.With(ctx => ctx.User, new ClaimsPrincipal(new ClaimsIdentity(claims)))
				.Create();

			var routeData = _fixture.Create<RouteData>();
			var actionDescriptor = new ActionDescriptor();

			if (appliedRuleAttribute != null)
			{
				var metadataList = new List<AppliedRuleAttribute> { appliedRuleAttribute };
				actionDescriptor.EndpointMetadata = metadataList.Cast<object>().ToList();
			}

			var context = new ActionExecutingContext(
				new ActionContext(httpContext, routeData, actionDescriptor),
				new List<IFilterMetadata>(),
				new Dictionary<string, object?>(),
				It.IsAny<object>());

			return context;
		}

		public static IEnumerable<object[]> AppliedRuleAttributeForFilter()
			=> new List<object[]>
			{
					new object[]
					{
						null,
					},
					new object[]
					{
						new AppliedRuleAttribute(),
					}
			};
	}
}
