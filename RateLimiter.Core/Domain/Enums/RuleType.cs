namespace RateLimiter.Core.Domain.Enums
{
	public enum RuleType
	{
		RequestPerTimeSpan = 1,
		TimeSpanPassedSinceLastCall = 2,
	}
}
