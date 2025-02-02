namespace Crexi.RateLimiter.Rule.Constants;

internal class ResultConstants
{
    internal const int DefaultFailureResponseCode = 429;
    internal static readonly (bool success, int? responseCode) SuccessResponse = (true, null);
}