namespace RateLimiterWeb.RateLimiting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RateLimitingAttribute : Attribute
    {
        public string GroupName { get; set; }
        public RateLimitingAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}
