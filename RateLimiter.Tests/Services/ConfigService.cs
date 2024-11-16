using Microsoft.EntityFrameworkCore;
using RateLimiter.Data;
using RateLimiter.Data.Interfaces;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class ConfigService : IConfigService
    {
        private readonly RateLimiterDbContext _context;

        public ConfigService(RateLimiterDbContext context)
        {
            _context = context;

            // Make sure statuses are in the db.  Should be handled in a db initializer but ran out of time.
            var statusesCount = _context.Statuses.Count();

            if (statusesCount == 0)
            {
                foreach (var status in CodeValues.Statuses)
                {
                    _context.Statuses.Add(status);
                }
                _context.SaveChanges();
            }
        }
        public async Task Reset()
        {
            foreach (var request in _context.Requests)
            {
                _context.Requests.Remove(request);
            }
            foreach (var resource in  _context.Resources)
            {
                _context.Resources.Remove(resource);
            }
            foreach (var user in _context.Users)
            {
                _context.Users.Remove(user);
            }
            await _context.SaveChangesAsync();
        }
        public async Task SeedResources(List<Resource> resources)
        {
            foreach (var resource in resources)
            {
                _context.Resources.Add(resource);
            }
            await _context.SaveChangesAsync();
        }
        public async Task SeedUsers(List<User> users)
        {
            foreach (var user in users)
            {
                _context.Users.Add(user);
            }
            await _context.SaveChangesAsync();
        }
        public async Task SeedRequests(List<Request> requests)
        {
            foreach (var request in requests)
            {
                _context.Add(request);
            };
            await _context.SaveChangesAsync();
        }
    }
}
