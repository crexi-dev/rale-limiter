using FluentValidation;
using RateLimiter.Models.Apis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Validators
{
    /// <summary>
    /// Validation class for rate limit checking request.
    /// </summary>
    public class RateLimitRequestValidator : AbstractValidator<RateLimitRequest>
    {
        public RateLimitRequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ResourceId).NotEmpty();
        }
    }
}
