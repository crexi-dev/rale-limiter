namespace Example.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class UsOriginUserAttribute : Attribute, IRateLimitAttribute
{
    public int Requests { get; }
    public TimeSpan TimeSpan { get; }
    public string ErrorMessage { get; }

    public UsOriginUserAttribute(int milliseconds, int requests)
    {
        Requests = requests;
        TimeSpan = TimeSpan.FromMilliseconds(milliseconds);
        ErrorMessage = "As US based user you have reached the number of requests for the configured time frame. Please try again later.";
    }

    public UsOriginUserAttribute(int milliseconds, string errorMessage)
    {
        TimeSpan = TimeSpan.FromMilliseconds(milliseconds);
        ErrorMessage = errorMessage;
    }

}