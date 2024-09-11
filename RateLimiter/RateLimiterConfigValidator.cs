using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace RateLimiter;

public class RateLimiterConfigValidator: AbstractValidator<RateLimiterConfig>, IValidateOptions<RateLimiterConfig>
{
    public RateLimiterConfigValidator()
    {
        RuleFor(c => c.CacheExpirationInMinutes).GreaterThan(0);
    }

    public ValidateOptionsResult Validate(string? name, RateLimiterConfig options)
    {
        ValidationResult validationResult = this.Validate(options);
        return !validationResult.IsValid
            ? ValidateOptionsResult.Fail(
                validationResult.Errors.Select<ValidationFailure, string>(
                    (Func<ValidationFailure, string>)(x => x.ErrorMessage)))
            : ValidateOptionsResult.Success;
    }
}