using M42.Data.Repositories;
using RateLimiter.Data.Interfaces;
using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    public class StatusesDataService : IDataService<Status>
    {
        private readonly DbRepository<Status> _statusesRepository;

        public StatusesDataService(DbRepository<Status> statusesRepository)
        {
            _statusesRepository = statusesRepository;
        }

        public async Task<List<Status>> Get()
        {
            throw new NotImplementedException();
        }
        public async Task<List<Status>> Get(BaseModel searchCriteria)
        {
            throw new NotImplementedException();
        }
        public async Task<Status> Get(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<Status> Get(string identifier)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Add(Status user)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Update(int id, Status user)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
