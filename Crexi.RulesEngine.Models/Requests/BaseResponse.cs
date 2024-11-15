using Crexi.RulesService.Models.Enums;

namespace Crexi.RulesService.Models.Requests;

public class BaseResponse
{
    public ResponseCodeEnum ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public string? RuleUsed { get; set; }
    public List<RulesResult> RulesResults{ get; set; }
    public BaseResponse()
    {
        ResponseCode = ResponseCodeEnum.Success;
        RulesResults = new();
    }


}
