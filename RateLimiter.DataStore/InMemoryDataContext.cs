using Microsoft.EntityFrameworkCore;
using RateLimiter.DataStore.Entities;
using RateLimiter.DataStore.Interfaces;

namespace RateLimiter.DataStore
{
    /// <summary>
    /// In-Memory data context implementation
    /// </summary>
    public class InMemoryDataContext : DbContext, IDataContext
    {
        public DbSet<RequestLog> RequestLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // In-Memory database used for simplicity
            options.UseInMemoryDatabase("RateLimiterDb");
        }

        void IDataContext.SaveChanges()
        {
            base.SaveChanges();
        }
    }
}
