namespace RateLimiter.Models.Enums
{
    public enum ResponseCodeEnum
    {
        Success = 100,
        SystemError = 500,
        ValidationError = 600
    }

   public enum RatePeriodTypeEnum
    {
        Seconds = 1,
        Minutes = 60,
        Hours = 3600
    }
}
