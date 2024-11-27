using RateLimiter.Core.Domain.Entity;
using System.Collections.Concurrent;

namespace RateLimiter.DataAccess.Repository.Implementation
{
	public class RequestRepository : IRequestRepository
	{
		private readonly ConcurrentDictionary<Guid, List<Request>> _storage = new();

		public Task AddRequestAsync(Request request)
		{
			_storage.AddOrUpdate(request.Id, new List<Request> { request }, (key, existingRequests) =>
			{
				existingRequests.Add(request);
				return existingRequests;
			});

			return Task.CompletedTask;
		} 

		public Task<List<Request>> GetRequestsAsync(Guid id)
		{
			_storage.TryGetValue(id, out var requests);
			var reesult = requests ?? new List<Request>();

			return Task.FromResult(reesult);
		}

		public Task CleanupOldRequestsAsync(Guid id, TimeSpan interval)
		{
			if (!_storage.ContainsKey(id)) 
			{
				return Task.CompletedTask;
			}

			var actualRequests = _storage[id].Where(request => DateTime.UtcNow - request.DateTime <= interval).ToList();
			_storage[id] = actualRequests;

			return Task.CompletedTask;
		}
	}
}
