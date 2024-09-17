using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RateLimiter.Models;

namespace RateLimiter.Repositories
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) 
            :base(options) 
        {                        
        }
        
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Rule> Rules { get; set; }    
        public DbSet<ResourceRule> ResourceRules { get; set; }
        public DbSet<ResourceVisitLog> ResourceVisitLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resource>().HasKey(x => x.Id);
            modelBuilder.Entity<Resource>().Property<int>(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Rule>().HasKey(x => x.Id);
            modelBuilder.Entity<Rule>().Property<int>(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ResourceRule>().HasKey(x => x.Id);
            modelBuilder.Entity<ResourceRule>().Property<int>(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ResourceVisitLog>().HasKey(x => x.Id);
            modelBuilder.Entity<ResourceVisitLog>().Property<int>(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}
