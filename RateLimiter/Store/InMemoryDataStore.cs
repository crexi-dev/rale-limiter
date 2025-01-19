using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Store
{
    public class InMemoryDataStore : IDataStore
    {
        private readonly Dictionary<string, List<ClientRequestModel>> _clientRequests = [];

        public void AddClientRequest(string clientToken, string uri)
        {
            var clientRequest = new ClientRequestModel(uri, DateTime.UtcNow);

            if (_clientRequests.TryGetValue(clientToken, out var clientRequests))
            {
                clientRequests.Add(clientRequest);
                return;
            }

            _clientRequests.TryAdd(clientToken, [clientRequest]);
        }

        public IEnumerable<ClientRequestModel> GetClientRequests(string clientToken, DateTime after)
        {
            if (!_clientRequests.TryGetValue(clientToken, out var clientRequests))
                return [];

            return clientRequests.Where(request => request.Timestamp > after);
        }

        public ClientRequestModel? GetLastRequestByClient(string clientToken)
        {
            if (!_clientRequests.TryGetValue(clientToken, out var clientRequests))
                return null;

            return clientRequests.Last();
        }

    }
}
