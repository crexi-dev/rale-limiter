using Microsoft.EntityFrameworkCore;
using RateLimiter.Data.Models;

namespace RateLimiter.Data.Contexts
{
    public class RateLimiterDbContext : DbContext
    {
        public RateLimiterDbContext(DbContextOptions<RateLimiterDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<LimiterRule> LimiterRules { get; set; }
    }
}
