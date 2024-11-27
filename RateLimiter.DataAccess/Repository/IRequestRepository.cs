using RateLimiter.Core.Domain.Entity;

namespace RateLimiter.DataAccess.Repository
{
	public interface IRequestRepository
	{
		Task AddRequestAsync(Request request);
		Task<List<Request>> GetRequestsAsync(Guid id);
		Task CleanupOldRequestsAsync(Guid id, TimeSpan interval);
	}
}
