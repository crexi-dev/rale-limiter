﻿using M42.Data.Repositories;
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
        private readonly DbRepository<Request> _requestsRepository;

        public RequestsDataService(DbRepository<Request> requestsRepository)
        {
            _requestsRepository = requestsRepository;
        }

        public async Task<List<Request>> GetAllAsync()
        {
            string[] includes = new string[] { "" };

            var requests = await _requestsRepository.GetAllAsync(includes);

            return requests;
        }
        public async Task<Request> SingleAsync(int id)
        {
            string[] includes = new string[] { "" };

            var request = await _requestsRepository.SingleAsync(id, includes);

            return request;
        }
        public async Task<Request> SingleAsync(string identifier)
        {
            string[] includes = new string[] { "" };

            var request = await _requestsRepository.SingleAsync(identifier, includes);

            return request;
        }
        public async Task<bool> AddAsync(Request request)
        {
            var newRequest = await _requestsRepository.AddAsync(request);

            return true;
        }
        public async Task<bool> UpdateAsync(int id, Request request)
        {
            throw new NotImplementedException("Cannot update a request.");
        }
        public async Task<bool> RemoveAsync(int id)
        {
            throw new NotImplementedException("Cannot remove a request.");
        }
    }
}
