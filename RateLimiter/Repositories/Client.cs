using System;
using System.Collections.Generic;
using RateLimiter.Enums;
using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Repositories
{
    public class Client : IClient
    {
        private readonly List<DateTime> _loggedTimes;
        private readonly string _accessToken;
        private readonly Region _region;

        public Client(string AccessToken)
        {
            _accessToken = AccessToken;

            _loggedTimes = new List<DateTime>();

            _region = GetRegionFromAccessToken();

        }

        /// <summary>
        /// Returns the client's region based on the token.
        /// </summary>
        /// <returns>Returns Region Enum.</returns>
        private Region GetRegionFromAccessToken()
        {
            //The assumption is the token is in the format of Region.xxxx where Region can be either US, EU, ASIA etc.
            var regionParsed = (_accessToken.Split('.'))[0];

            return Enum.TryParse(regionParsed, out Region region) ? region : Region.None;
        }

        /// <summary>
        /// Return the client's region.
        /// </summary>
        /// <returns>Return Region Enum.</returns>
        public Region ReturnRegion()
        {
            return _region;
        }

        /// <summary>
        /// Return the collection of LoggedTimes for this client.
        /// </summary>
        /// <returns>Returns List<DateTime></returns>
        public List<DateTime> ReturnLoggedTimes()
        {
            return _loggedTimes;
        }

        /// <summary>
        /// Add the current time (UTC) to the collection.
        /// </summary>
        public void AddRequest()
        {
            _loggedTimes.Add(DateTime.UtcNow);
        }
    }
}
