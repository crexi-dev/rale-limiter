using FluentValidation;
using RateLimiter.Api.Models.Request;
using RateLimiter.Core.Domain.Enums;

namespace RateLimiter.Api.Infrastructure.Validators
{
	public class TokenRequestValidator : AbstractValidator<CreateTokenRequestModel>
	{
		public TokenRequestValidator()
		{
			RuleFor(x => x.Type).Must(ValidateTokenRegionType);
		}

		private bool ValidateTokenRegionType(RegionType type)
		{
			var IsValidRegionType = RegionType.None != type;

			if (!IsValidRegionType)
			{
				throw new ArgumentException(string.Format(
					"{0} should not be set up in {1}. Region Types are valid: {2}",
					nameof(RegionType),
					RegionType.None,
					string.Join(", ", Enum.GetValues(typeof(RegionType)).Cast<RegionType>().ToList().Where(x => x != RegionType.None))));
			}

			return IsValidRegionType;
		}
	}
}
