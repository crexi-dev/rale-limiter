namespace Crexi.RulesService.Models.Requests
{
    public class GetRulesRequest
    {
        public string RulesFile { get; set; } = null!;
        public string Workflow { get; set; } = null!;
        public object[] Input { get; set; } = null!;
        public DateOnly Date { get; set; }
        
    }
}
