using RateLimiter.Models;
using System;
using System.Collections.Generic;

namespace RateLimiter.Store
{
    public interface IDataStore
    {
        void AddClientRequest(string clientToken, string uri);

        IEnumerable<ClientRequestModel> GetClientRequests(string clientToken, DateTime after);

        ClientRequestModel? GetLastRequestByClient(string clientToken);
    }
}
