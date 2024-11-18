using RulesService.Models.Enums;

namespace RulesService.Models.Requests;

public class GetRulesResponse
{
    public ResponseCodeEnum ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public string? RuleUsed { get; set; }
    public List<RulesResult> RulesResults{ get; set; }
    public GetRulesResponse()
    {
        ResponseCode = ResponseCodeEnum.Success;
        RulesResults = new();
    }


}
