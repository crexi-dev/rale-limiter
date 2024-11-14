using Microsoft.EntityFrameworkCore;
using RateLimiter.Data;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M42.Data.Repositories
{
    public class DbRepository<Entity> where Entity : BaseModel
    {
        private readonly RateLimiterDbContext _context;
        private DbSet<Entity> _dbSet;

        public DbRepository(RateLimiterDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Entity>();
        }

        public async Task<List<Entity>> GetAllAsync(string[] includes)
        {
            var dbSet = _dbSet.AsQueryable();

            foreach (var include in includes)
            {
                if (include != "")
                {
                    dbSet = dbSet.Include(include);
                }
            }

            return await dbSet.ToListAsync();
        }

        public async Task<Entity> SingleAsync(int id, string[] includes)
        {

            var dbSet = _dbSet.AsQueryable();

            foreach (var include in includes)
            {
                if (include != "")
                {
                    dbSet = dbSet.Include(include);
                }
            }
               

            var entity = await dbSet.SingleOrDefaultAsync(e => e.Id == id);

            if (entity == null)
            {
                throw new InvalidOperationException("Invalid Id value");
            }

            return entity;
        }

        public async Task<Entity> SingleAsync(string identifier, string[] includes)
        {

            var dbSet = _dbSet.AsQueryable();

            foreach (var include in includes)
            {
                if (include != "")
                {
                    dbSet = dbSet.Include(include);
                }
            }

            var entity = await dbSet.SingleOrDefaultAsync(e => e.Identifier == identifier);

            if (entity == null)
            {
                throw new InvalidOperationException("Invalid identifier value");
            }

            return entity;
        }
        
        public async Task<Entity> SingleOrDefaultAsync(int id)
        {
            var dbSet = _dbSet.AsQueryable();
            var entity = await dbSet.SingleOrDefaultAsync(e => e.Id == id);

            if (entity == null)
            {
                throw new Exception("Invalid id");
            }

            return entity;
        }
        
        public async Task<bool> Add(Entity entity)
        {
            var result = _dbSet.Add(entity);

            return true;
        }

        public async Task<bool> Update(Entity entity)
        {
            var result = _dbSet.Update(entity);

            return true;
        }

        public async Task<bool> Remove(Entity entity)
        {
            throw new NotImplementedException("Audit data should not be deleted.");
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}

