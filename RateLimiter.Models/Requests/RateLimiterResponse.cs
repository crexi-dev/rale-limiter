using RateLimiter.Models.Enums;
using RulesService.Models;
using RulesService.Models.Enums;

namespace RateLimiter.Models.Requests;

public class RateLimiterResponse
{
    public ResponseCodeEnum ResponseCode { get; set; }
    public RulesServiceResponseCodeEnum? RuleServiceResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public bool IsRateExceeded { get; set; }
   
    public RateLimiterRule? RateLimiterRule { get; set; }

    public override string ToString()
    {
        return $"ResponseCode : {ResponseCode}, RuleServiceResponseCode : {RuleServiceResponseCode}, ResponseMessage : {ResponseMessage}, IsRateExceeded : {IsRateExceeded}, RateLimiterRule : {{{RateLimiterRule}}}";
    }

}
