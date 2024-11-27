using RateLimiter.BusinessLogic.Models;
using RateLimiter.Core.Domain.Entity;
using RateLimiter.DataAccess.Repository;

namespace RateLimiter.BusinessLogic.Services.RateLimiter.Rules
{
	public abstract class RequestPerTimeSpanRuleBase
	{
		protected readonly IRequestRepository _requestRepository;

		protected RequestPerTimeSpanRuleBase(IRequestRepository requestRepository)
		{
			_requestRepository = requestRepository;
		}

		public virtual int RequestsCount => 3;
		public virtual TimeSpan Interval => TimeSpan.FromMinutes(1);

		public virtual async Task<bool> ApplyToRequest(RequestDto requestModel)
		{
			await _requestRepository.CleanupOldRequestsAsync(requestModel.Id, Interval);
			var requests = await _requestRepository.GetRequestsAsync(requestModel.Id);

			if (requests.Count == RequestsCount)
			{
				return false;
			}

			await _requestRepository.AddRequestAsync(new Request
			{
				Id = requestModel.Id,
				RegionType = requestModel.RegionType,
				UrlPath = requestModel.UrlPath,
				DateTime = DateTime.UtcNow,
			});

			return true;
		}
	}
}
