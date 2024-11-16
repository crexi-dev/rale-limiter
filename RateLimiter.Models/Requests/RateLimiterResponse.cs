using RateLimiter.Models.Enums;

namespace RateLimiter.Models.Requests;

public class RateLimiterResponse
{
    public ResponseCodeEnum ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public string? RuleUsed { get; set; }
    public bool IsRateExceeded { get; set; }
    public bool IsRequestAllowed { get; set; }
    public double RateLimit { get; set; }
    public RateSpanTypeEnum RatePeriodType { get; set; }
    public double CurrentPeriodRequestsNumber {  get; set; }

}
