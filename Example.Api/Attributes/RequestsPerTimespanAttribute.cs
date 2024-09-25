namespace Example.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequestsPerTimespanAttribute : Attribute, IRateLimitAttribute
{
    public int Rate { get; }
    public int Milliseconds { get; }
    public string ErrorMessage { get; }

    public RequestsPerTimespanAttribute(int rate, int milliseconds)
    {
        Rate = rate;
        Milliseconds = milliseconds;
        ErrorMessage = "Allowed requests exceeded";
    }

    public RequestsPerTimespanAttribute(int rate, int milliseconds, string errorMessage)
    {
        Rate = rate;
        Milliseconds = milliseconds;
        ErrorMessage = errorMessage;
    }

}

public interface IRateLimitAttribute
{
    string ErrorMessage { get; }

}