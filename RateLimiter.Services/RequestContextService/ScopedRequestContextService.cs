using RateLimiter.Common.Exceptions;

namespace RateLimiter.Services.RequestContextService;

public class ScopedRequestContextService(RequestContext context) : IRequestContextService
{
    #region Initialize
    protected RequestContext context = context;
    #endregion

    #region IRequestContextService
    public Task<RequestContext> GetRequestContext()
    {
        ValidateRequestContext();

        return Task.FromResult(context);
    }

    public Task SetRequestContext(RequestContext context)
    {
        this.context = this.context with
        {
            Token = context.Token ?? this.context.Token,
            OriginIsoCountryCode = context.OriginIsoCountryCode ?? this.context.OriginIsoCountryCode
        };

        ValidateRequestContext();

        return Task.CompletedTask;
    }
    #endregion

    #region Validate

    private void ValidateRequestContext()
    {
        if (string.IsNullOrWhiteSpace(context.Token) || string.IsNullOrWhiteSpace(context.Resource))
        {
            throw new InvalidRequestContextException();
        }
    }

    #endregion
}
