using RateLimiter.Interfaces;
using System;

namespace RateLimiter.Storage
{
    public class TokenBasedStorageEntry : IRateLimiterStorageEntry
    {
        /// <summary>
        /// Amount of tokens left
        /// </summary>
        public int Tokens { get; set; }

        /// <summary>
        /// Last refill time
        /// </summary>
        public DateTime LastRefill { get; set; }
    }
}
