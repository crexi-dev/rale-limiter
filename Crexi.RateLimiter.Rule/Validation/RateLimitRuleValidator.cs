using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Crexi.RateLimiter.Rule.Enum;
using Crexi.RateLimiter.Rule.Model;
using Microsoft.Extensions.Options;
using Crexi.RateLimiter.Rule.Configuration.Sections;

namespace Crexi.RateLimiter.Rule.Validation
{
    public class RateLimitRuleValidator : AbstractValidator<RateLimitRule>
    {
        public RateLimitRuleValidator(IEnumerable<EndpointDataSource> endpoints, IOptions<RateLimiterConfiguration> options)
        {
            RuleFor(r => r.Resource)
                .NotEmpty().WithMessage("Must provide a target resource.")
                .Must(r => endpoints.Any(e => e.Endpoints.Any(ee => ee.DisplayName == r))).WithMessage("Not a valid resource for this service.");
            RuleFor(r => r.Timespan)
                .Must(t => t < options.Value.MaxTimeSpanMinutes * 60000)
                .WithMessage($"Requested timespan exceeds the max configured limit of {options.Value.MaxTimeSpanMinutes} minutes.");
            RuleFor(r => r.MaxCallCount)
                .NotNull()
                .When(r => r.EvaluationType == EvaluationType.CallsDuringTimespan)
                .WithMessage("Must provide max calls when evaluating CallsDuringTimespan.");
            /*
             * NOTE: No "> 0" rule for timespan.  Allowing for a "no access" rule by setting timespan to 0.
             * Given field is required and non-nullable, this introduces the potential for user error.
             * For the purposes here, assuming only smart, cautious people will use this to set rules.
            */
            RuleFor(r => r.EvaluationType)
                .NotEmpty()
                .Must(et => System.Enum.IsDefined(typeof(EvaluationType), et))
                .WithMessage("Must provide a valid evaluation type.");
            RuleFor(r => r.EffectiveWindowStartUtc)
                .NotNull().When(r => r.EffectiveWindowEndUtc is not null).WithMessage("EffectiveWindowStartUtc must be provided when EffectiveWindowEndUtc is provided.");
            RuleFor(r => r.EffectiveWindowEndUtc)
                .NotNull().When(r => r.EffectiveWindowStartUtc is not null).WithMessage("EffectiveWindowEndUtc must be provided when EffectiveWindowStartUtc is provided.");
            RuleFor(r => r.EffectiveWindowStartUtc)
                .Must((r, windowStart) => windowStart < r.EffectiveWindowEndUtc).When(r => r.EffectiveWindowStartUtc is not null && r.EffectiveWindowEndUtc is not null)
                    .WithMessage("EffectiveWindowStartUtc must be before EffectiveWindowEndUtc.");
        }
    }
}
