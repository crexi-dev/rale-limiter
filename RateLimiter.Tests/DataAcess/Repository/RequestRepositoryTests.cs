using AutoFixture;
using Moq;
using RateLimiter.Core.Domain.Entity;
using RateLimiter.DataAccess.Repository.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiter.Tests.DataAcess.Repository
{
	public class RequestRepositoryTests
	{
		private const string DefaultActualUrlPath = "test/actualResource";

		private readonly RequestRepository _requestRepository;

		public RequestRepositoryTests()
		{
			_requestRepository = new RequestRepository();
		}

		[Fact]
		public async Task AddRequestAsync_ShouldAddRequestToStorage()
		{
			// Arrange
			var requestId = Guid.NewGuid();
			var request = new Fixture()
				.Build<Request>()
				.With(x => x.Id, requestId)
				.Create();

			// Act
			await _requestRepository.AddRequestAsync(request);

			// Assert
			var storedRequests = await _requestRepository.GetRequestsAsync(requestId);
			Assert.Single(storedRequests);
			Assert.Equal(request, storedRequests.First());
		}

		[Fact]
		public async Task GetRequestsAsync_WhenNoRequestsExist_ShouldReturnEmptyList()
		{
			// Arrange
			var requestId = Guid.NewGuid();

			// Act
			var storedRequests = await _requestRepository.GetRequestsAsync(requestId);

			// Assert
			Assert.Empty(storedRequests);
		}

		[Fact]
		public async Task CleanupOldRequestsAsync_WhenNoRequestsExist_ShouldNotThrowException()
		{
			// Arrange
			var requestId = Guid.NewGuid();

			// Act
			await _requestRepository.CleanupOldRequestsAsync(requestId, It.IsAny<TimeSpan>());

			// Assert
			var storedRequests = await _requestRepository.GetRequestsAsync(requestId);
			Assert.Empty(storedRequests);
		}

		[Theory]
		[MemberData(nameof(DataForMixedOldAndActualRequests))]
		public async Task CleanupOldRequestsAsync_WhenStorageHasOldAndActualRequests_ShouldRemoveOldRequests(PreparedMixedData data)
		{
			// Arrange
			var interval = TimeSpan.FromMinutes(3);
			var requestId = data.Requests.First().Id;

			var tasks = data.Requests.Select(_requestRepository.AddRequestAsync).ToList();
			await Task.WhenAll(tasks);

			// Act
			await _requestRepository.CleanupOldRequestsAsync(requestId, interval);

			// Assert
			var storedRequests = await _requestRepository.GetRequestsAsync(requestId);
			Assert.Equal(data.ExpectedActualRequestCount, storedRequests.Count);

			if (storedRequests.Any())
			{
				Assert.Equal(DefaultActualUrlPath, storedRequests.Single().UrlPath);
			}
		}

		public static IEnumerable<object[]> DataForMixedOldAndActualRequests()
		{
			var requestId = Guid.NewGuid();

			return new List<object[]>
			{
				new object[]
				{
					new PreparedMixedData
					{
						Requests = new List<Request>
						{
							new Fixture()
								.Build<Request>()
								.With(x => x.Id, requestId)
								.With(x => x.DateTime, DateTime.UtcNow.AddMinutes(-10))
								.Create(),
							new Fixture()
								.Build<Request>()
								.With(x => x.Id, requestId)
								.With(x => x.DateTime, DateTime.UtcNow.AddMinutes(-6))
								.Create(),
							new Fixture()
								.Build<Request>()
								.With(x => x.Id, requestId)
								.With(x => x.DateTime, DateTime.UtcNow.AddMinutes(-5))
								.Create(),
						},
						ExpectedActualRequestCount = 0,
					}
				},
				new object[]
				{
					new PreparedMixedData
					{
						Requests =  new List<Request>
						{
							new Fixture()
								.Build<Request>()
								.With(x => x.Id, requestId)
								.With(x => x.DateTime, DateTime.UtcNow.AddMinutes(-7))
								.Create(),
							new Fixture()
								.Build<Request>()
								.With(x => x.Id, requestId)
								.With(x => x.UrlPath, DefaultActualUrlPath)
								.With(x => x.DateTime, DateTime.UtcNow.AddMinutes(-1))
								.Create(),
						},
						ExpectedActualRequestCount = 1,
					}
				},
				new object[]
				{
					new PreparedMixedData
					{
						Requests = new List<Request>
						{
							new Fixture()
								.Build<Request>()
								.With(x => x.Id, requestId)
								.With(x => x.UrlPath, DefaultActualUrlPath)
								.With(x => x.DateTime, DateTime.UtcNow.AddMinutes(-2))
								.Create(),
						},
						ExpectedActualRequestCount = 1,
					}
					
				},
			};
		}
	}

	public class PreparedMixedData
	{
		public List<Request> Requests { get; set; }
		public int ExpectedActualRequestCount { get; set; }
	}
}
