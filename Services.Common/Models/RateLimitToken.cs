using System;

namespace Services.Common.Models;

public class RateLimitToken
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Region { get; set; }
    public string Resource { get; set; }
}