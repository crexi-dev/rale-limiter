using Microsoft.EntityFrameworkCore;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Data
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
    }
}
