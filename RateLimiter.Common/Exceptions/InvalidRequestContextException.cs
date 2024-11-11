namespace RateLimiter.Common.Exceptions;

public class InvalidRequestContextException(string message) : Exception(message)
{
    public InvalidRequestContextException() : this("The request context is not properly configured. It is missing required information.") { }
}
