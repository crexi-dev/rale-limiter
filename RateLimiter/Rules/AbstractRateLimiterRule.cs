using RateLimiter.Interfaces;
using System;

namespace RateLimiter.Rules;

/// <summary>
/// Abstract Rate Limiter Rule
/// </summary>
public abstract class AbstractRateLimiterRule<T, U> : IRateLimiterRule<U>, IRateLimiterRule where T : IRateLimiterStorageEntry where U: IRateLimiterResult
{
    #region Constructor
    /// <summary>
    /// The base class of a rule takes the storage as a parameter
    /// </summary>
    /// <param name="storage"></param>
    public AbstractRateLimiterRule(IRateLimiterStorage storage)
    {
        _storage = storage;
    }
    #endregion

    #region Abstracts
    /// <summary>
    /// Whether if the request is allowerd or not
    /// </summary>
    /// <returns></returns>
    public abstract U IsRequestAllowed();
    /// <summary>
    /// Calculates the time in seconds that the client has to wait for another request
    /// </summary>
    /// <param name="rateLimitResult"></param>
    /// <returns></returns>
    protected abstract double CaculateRetryAfter(U rateLimitResult);
    /// <summary>
    /// Gets or create a storage entry using a key
    /// </summary>
    /// <returns></returns>
    protected abstract T GetOrCreateStorageEntry();
    /// <summary>
    /// The key for a storage entry
    /// </summary>
    protected abstract string Key { get; }
    #endregion

    #region Privates
    /// <summary>
    /// Storage
    /// </summary>
    private readonly IRateLimiterStorage _storage;
    #endregion

    #region Properties
    protected IRateLimiterStorage Storage => _storage;
    /// <summary>
    /// Parsed token from the expected header
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// IRateLimiterRule implementation
    /// </summary>
    /// <returns></returns>
    IRateLimiterResult IRateLimiterRule.IsRequestAllowed() => IsRequestAllowed();
    #endregion

    #region Implementation
    /// <summary>
    /// Sniffs a request and if its not allowed by a given rule, it will return the proper error with the retry-after header
    /// </summary>
    /// <param name="context"></param>
    public Tuple<bool, double> Execute(string token)
    {
        AccessToken = token;

        var result = IsRequestAllowed();

        if (!result.Success)
        {
            return new Tuple<bool, double>(false, CaculateRetryAfter(result));
        }

        return new Tuple<bool, double>(true, 0);
    }
    #endregion
}
