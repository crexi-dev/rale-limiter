namespace RateLimiter.Models.Enums
{
    public enum ResponseCodeEnum
    {
        Success = 100,
        SystemError = 500,
        ValidationError = 600,
        RulesEngineError = 700
    }

   public enum RateSpanTypeEnum
    {
        Second = 1,
        Minute = 60,
        Hour = 3600
    }
}
