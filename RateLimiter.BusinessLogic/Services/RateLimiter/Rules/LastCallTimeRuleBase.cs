using RateLimiter.BusinessLogic.Models;
using RateLimiter.Core.Domain.Entity;
using RateLimiter.DataAccess.Repository;

namespace RateLimiter.BusinessLogic.Services.RateLimiter.Rules
{
	public abstract class LastCallTimeRuleBase
	{
		protected readonly IRequestRepository _requestRepository;

		protected LastCallTimeRuleBase(IRequestRepository requestRepository)
		{
			_requestRepository = requestRepository;
		}

		public virtual TimeSpan MinimumInterval => TimeSpan.FromMinutes(2);

		public virtual async Task<bool> ApplyToRequest(RequestDto requestModel)
		{
			var requests = await _requestRepository.GetRequestsAsync(requestModel.Id);
			if (!requests.Any())
			{
				return await AddRequestToStorage(new Request
				{
					Id = requestModel.Id,
					RegionType = requestModel.RegionType,
					UrlPath = requestModel.UrlPath,
					DateTime = DateTime.UtcNow,
				});
			}

			var timeSinceLastRequest = DateTime.UtcNow - requests.Last().DateTime;
			if (timeSinceLastRequest > MinimumInterval)
			{
				return await AddRequestToStorage(new Request
				{
					Id = requestModel.Id,
					RegionType = requestModel.RegionType,
					UrlPath = requestModel.UrlPath,
					DateTime = DateTime.UtcNow,
				});
			}

			return false;
		}

		private async Task<bool> AddRequestToStorage(Request requestModel)
		{
			await _requestRepository.AddRequestAsync(requestModel);
			return true;
		}
	}
}
