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
    public class RequestsDataService : IDataService<Request>
    {
        private readonly DbRepository<Request> _requestRepository;

        public RequestsDataService(DbRepository<Request> requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task<List<Request>> Get()
        {
            throw new NotImplementedException();
        }
        public async Task<List<Request>> Get(BaseModel searchCriteria)
        {
            throw new NotImplementedException();
        }
        public async Task<Request> Get(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<Request> Get(string identifier)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Add(Request request)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Update(int id, Request request)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
