namespace RulesService.Models.Enums
{
    public enum RulesServiceResponseCodeEnum
    {
        Success = 100,
        SystemError = 500,
        ValidationError = 600,
        WorkflowError = 700,
        ScopedParameterError = 800,
        ExpressionParserError = 700,
        RulesEngineError = 900,
    }


}
