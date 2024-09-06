namespace RateLimiter.Config
{
    public enum LimitType : short
    {
        NotSet = 0,

        /// <summary>
        /// X requests per timespan;
        /// </summary>
        TimeWindow = 1,


        /// <summary>
        /// a certain timespan has passed since the last call
        /// </summary>
        RequestSpacing = 2
    }
}
