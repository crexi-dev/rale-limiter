using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Data.Interfaces
{
    public interface IDataService<T> where T : BaseModel
    {
        public Task<List<T>> Get();

        public Task<List<T>> Get(BaseModel searchCriteria);

        public Task<T> Get(int id);

        public Task<T> Get(string identifier);

        public Task<bool> Add(T entity);
        public Task<bool> Update(int id, T entity);
        public Task<bool> Delete(int id); 
    }
}
