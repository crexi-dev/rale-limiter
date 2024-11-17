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
        public async Task<List<Entity>> FindAsync(BaseModel searchCriteria, string[] includes)
        {
            var dbSet = _dbSet.AsQueryable();
            foreach (var include in includes)
            {
                if (include != "")
                {
                    dbSet = dbSet.Include(include);
                }
            }

            var entities = await dbSet.Where(x => searchCriteria.CreatedBy == null || x.CreatedBy == searchCriteria.CreatedBy).ToListAsync();


            return entities;
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
        
        public async Task<Entity?> SingleOrDefaultAsync(int id, string[] includes)
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

            return entity;
        }
        
        public async Task<bool> AddAsync(Entity entity)
        {
            // Need to add logic that insures unique Identifer


            var result = _dbSet.Add(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(Entity entity)
        {
            var existing = await _dbSet.SingleOrDefaultAsync(x => x.Id == entity.Id);

            if (existing == null)
            {
                throw new ArgumentException("Invalid id provided.");
            }

            var result = _dbSet.Update(entity);  // do i need to copy to existing?
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var entity = await _dbSet.SingleOrDefaultAsync(x => x.Id == id);

            if ( entity == null)
            {
                throw new ArgumentException("Invalid id provided.");
            }
            _context.Remove(entity);
            _context.SaveChanges();

            return true;
        }

    }
}

