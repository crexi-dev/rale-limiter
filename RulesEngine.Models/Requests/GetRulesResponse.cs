using RulesService.Models.Enums;

namespace RulesService.Models.Requests;

public class GetRulesResponse
{
    public RulesServiceResponseCodeEnum ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public string? RuleUsed { get; set; }
    public List<RulesResult> RulesResults{ get; set; }
    public GetRulesResponse()
    {
        ResponseCode = RulesServiceResponseCodeEnum.Success;
        RulesResults = new();
    }


}
