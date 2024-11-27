using RateLimiter.BusinessLogic.Models;
using RateLimiter.DataAccess.Repository;

namespace RateLimiter.BusinessLogic.Services.RateLimiter.Rules
{
	public abstract class RequestPerTimeSpanRuleBase
	{
		private readonly IRequestRepository _requestRepository;

		protected RequestPerTimeSpanRuleBase(IRequestRepository requestRepository)
		{
			_requestRepository = requestRepository;
		}

		public virtual int RequestsCount => 5;
		public virtual TimeSpan Interval => TimeSpan.FromMinutes(5);

		public virtual async Task<bool> ApplyToRequest(RequestDto requestModel)
		{
			throw new NotImplementedException();
		}
	}
}
