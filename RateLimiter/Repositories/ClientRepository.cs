using RateLimiter.Interfaces;
using System.Collections.Generic;

namespace RateLimiter.Repositories
{
    /// <summary>
    /// Repository for client data.
    /// </summary>
    public class ClientRepository : IClientRepository
    {
        private readonly Dictionary<string, string> _clientRegions = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRepository"/> class.
        /// </summary>
        public ClientRepository()
        {
            // Example data
            _clientRegions["client1"] = "US";
            _clientRegions["client2"] = "EU";
            _clientRegions["client3"] = "US";
            _clientRegions["client4"] = "EU";
        }

        public string GetClientRegionByAccessToken(string accessToken)
        {
            return _clientRegions.TryGetValue(accessToken, out var region) ? region : "Unknown";
        }
    }
}
