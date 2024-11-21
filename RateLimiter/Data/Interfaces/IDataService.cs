using RateLimiter.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RateLimiter.Data.Interfaces
{
    public interface IDataService<T> where T : BaseModel
    {
        public Task<List<T>> GetAllAsync();
        public Task<List<T>> FindAsync(BaseModel searchCriteria);
        public Task<T> SingleAsync(int id);
        public Task<T?> SingleOrDefaultAsync(int id);

        public Task<T> SingleAsync(string identifier);
        public Task<bool> AddAsync(T entity);
        public Task<bool> UpdateAsync(int id, T entity);
        public Task<bool> RemoveAsync(int id); 
    }
}
