namespace Example.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class EuOriginUserAttribute : Attribute, IRateLimitAttribute
{
    public TimeSpan TimeSpan { get; }
    public string ErrorMessage { get; }

    public EuOriginUserAttribute(int milliseconds)
    {
        TimeSpan = TimeSpan.FromMilliseconds(milliseconds);
        ErrorMessage = $"As EU based user you have to wait {TimeSpan.TotalSeconds} between each call. Please try again later.";
    }

    public EuOriginUserAttribute(int milliseconds, string errorMessage)
    {
        TimeSpan = TimeSpan.FromMilliseconds(milliseconds);
        ErrorMessage = errorMessage;
    }

}