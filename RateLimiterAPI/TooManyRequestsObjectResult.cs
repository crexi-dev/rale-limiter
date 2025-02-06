using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace RateLimiterAPI
{
    [DefaultStatusCode(DefaultStatusCode)]
    public class TooManyRequestsObjectResult : ObjectResult
    {
        private const int DefaultStatusCode = StatusCodes.Status429TooManyRequests;
        public TimeSpan? RetryAfter { get; }

        public TooManyRequestsObjectResult([ActionResultObjectValue] object value)
            : base(value)
        {
            StatusCode = DefaultStatusCode;
        }

        public TooManyRequestsObjectResult([ActionResultObjectValue] object value, int retryAfter)
            : this(value, TimeSpan.FromSeconds(retryAfter))
        {
        }

        public TooManyRequestsObjectResult([ActionResultObjectValue] object value, TimeSpan retryAfter)
            : this(value)
        {
            if (retryAfter < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(retryAfter), $"{nameof(retryAfter)} must be a non negative value");
            }

            RetryAfter = retryAfter;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (RetryAfter.HasValue)
            {
                context.HttpContext.Response.Headers.RetryAfter = Math.Round(RetryAfter.Value.TotalSeconds).ToString();
            }

            return base.ExecuteResultAsync(context);
        }
    }
}
