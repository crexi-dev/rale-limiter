using RulesService.Models.Requests;

namespace RulesService.Interfaces;

public interface IRulesService
{
    Task<GetRulesResponse> GetRulesAsync(GetRulesRequest request);
}
