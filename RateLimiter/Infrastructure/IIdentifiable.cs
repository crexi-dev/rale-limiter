namespace RateLimiter.Infrastructure;

public interface IIdentifiable<T>
{
    T Id { get; }
}