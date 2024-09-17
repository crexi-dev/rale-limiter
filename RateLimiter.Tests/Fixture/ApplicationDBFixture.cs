using Microsoft.EntityFrameworkCore;
using RateLimiter.Repositories;
using System;
using RateLimiter.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Tests.Fixture
{
    public class ApplicationDBFixture : IDisposable
    {
        private DbContextOptions<ApplicationDBContext> _contextOptions;
        public ApplicationDBContext DBContext { get; set; }

        public ApplicationDBFixture()
        {
            DbContextOptionsBuilder<ApplicationDBContext> builder = new DbContextOptionsBuilder<ApplicationDBContext>();
            builder.UseInMemoryDatabase($"InMemoryDatabase{Guid.NewGuid()}");
            this._contextOptions = builder.Options;
            this.DBContext = new ApplicationDBContext(this._contextOptions);
            this.Setup();
        }

        public void Dispose()
        {
            this._contextOptions = null;
            this.DBContext.Dispose();
        }

        private void Setup()
        {
            DBContext.Resources.AddRange(
                new Resource()
                {
                    Id = 1,
                    EndpointUrl = "www.cnn.com"
                },
                new Resource()
                {
                    Id = 2,
                    EndpointUrl = "www.bbc.com"
                },
                new Resource()
                {
                    Id = 3,
                    EndpointUrl = "www.netflix.com"
                });
            this.DBContext.SaveChanges();

            this.DBContext.Rules.AddRange(
                new Rule()
                {
                    Id = 1,
                    Interval = 5,
                    NumberOfRequestsAllowedPerInterval = 1,
                    NumberOfRequestAllowedPerIntervalActive = true,
                    TimeSinceLastRequestActive = false
                },
                new Rule()
                {
                    Id = 2,
                    NumberOfRequestsAllowedPerInterval = 2,
                    NumberOfRequestAllowedPerIntervalActive = false,
                    TimeSinceLastRequest = 5,
                    TimeSinceLastRequestActive = true
                },
                new Rule()
                {
                    Id = 3,
                    Interval = 10,
                    NumberOfRequestsAllowedPerInterval = 3,
                    NumberOfRequestAllowedPerIntervalActive = true,
                    TimeSinceLastRequest = 3,
                    TimeSinceLastRequestActive = true
                });
            this.DBContext.SaveChanges();

            this.DBContext.ResourceRules.AddRange(
                new ResourceRule()
                {
                    Id = 1,
                    ResourceId = 1,
                    RuleId = 1,
                    Active = true,
                },

                new ResourceRule()
                {
                    Id = 2,
                    ResourceId = 2,
                    RuleId = 1,
                    Active = true,
                },
                new ResourceRule()
                {
                    Id = 3,
                    ResourceId = 2,
                    RuleId = 2,
                    Active = true,
                },
                new ResourceRule()
                {
                    Id = 4, 
                    ResourceId = 3,
                    RuleId = 3,
                    Active = true,
                });
            this.DBContext.SaveChanges();
        }
    }
}
