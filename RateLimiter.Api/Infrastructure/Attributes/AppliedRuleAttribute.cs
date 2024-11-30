using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.Api.Infrastructure.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class AppliedRuleAttribute : Attribute
	{
		public RuleType[] RuleTypes { get; }

		public AppliedRuleAttribute(params RuleType[] ruleTypes)
		{
			RuleTypes = ruleTypes;
		}
	}
}
