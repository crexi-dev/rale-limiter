namespace Crexi.RulesService.Models.Requests
{
    public class BaseRequest
    {
        public string RuleFile { get; set; } = null!;
        public string Workflow { get; set; } = null!;
        public object[] Input { get; set; } = null!;
    }
}
