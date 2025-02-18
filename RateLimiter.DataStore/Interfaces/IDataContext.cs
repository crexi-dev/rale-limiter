using Microsoft.EntityFrameworkCore;
using RateLimiter.DataStore.Entities;

namespace RateLimiter.DataStore.Interfaces
{
    public interface IDataContext
    {
        public DbSet<RequestLog> RequestLogs { get; set; }

        void SaveChanges();
    }
}
