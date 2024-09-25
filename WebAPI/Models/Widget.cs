using RateLimiter.Infrastructure;
using WebAPI.Infrastructure;

namespace WebAPI.Models;

public class Widget : IIdentifiable<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }


    public Widget():this("new widget", "empty widget")
    {
    }

    public Widget(string name, string description, Guid id = default)
    {
        Id = id != default ? id : Guid.NewGuid();
        Name = name;
        Description = description;
    }
}