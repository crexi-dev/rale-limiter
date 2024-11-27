using RateLimiter.BusinessLogic.Models;
using RateLimiter.DataAccess.Repository;

namespace RateLimiter.BusinessLogic.Services.RateLimiter.Rules
{
	public abstract class LastCallTimeRuleBase
	{
		private readonly IRequestRepository _requestRepository;

		protected LastCallTimeRuleBase(IRequestRepository requestRepository)
		{
			_requestRepository = requestRepository;
		}

		public virtual async Task<bool> ApplyToRequest(RequestDto requestModel)
		{
			throw new NotImplementedException();
		}
	}
}
