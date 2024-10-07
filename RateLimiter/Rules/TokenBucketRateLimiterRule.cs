using RateLimiter.Interfaces;
using RateLimiter.Results;
using RateLimiter.Storage;
using System;

namespace RateLimiter.Rules
{
    /// <summary>
    /// Rule that allows requests to be controlled by a dynamic bucket. This buckets refills at a given rate allowing more requests
    /// </summary>
    public class TokenBucketRateLimiterRule : AbstractRateLimiterRule<TokenBasedStorageEntry, TokenBasedRateLimiterResult>
    {
        public TokenBucketRateLimiterRule(
            IRateLimiterStorage storage,
            int maxTokens, 
            int tokensPerRefill, 
            TimeSpan refillInterval
        ) : base(storage)
        {
            _maxTokens = maxTokens;
            _availableTokens = maxTokens;
            _tokensPerRefill = tokensPerRefill;
            _refillInterval = refillInterval;
            _lastRefill = DateTime.UtcNow;
        }

        private readonly int _maxTokens;          // Maximum tokens in the bucket (capacity)
        private int _availableTokens;             // Current number of available tokens
        private readonly TimeSpan _refillInterval; // Time interval between token refills
        private readonly int _tokensPerRefill;    // How many tokens are refilled per interval
        private DateTime _lastRefill;

        /// <summary>
        /// Storage Key
        /// </summary>
        protected override string Key => $"{nameof(TokenBucketRateLimiterRule)}_{AccessToken}";

        /// <summary>
        /// Gets or creates a new storage entry
        /// </summary>
        /// <returns></returns>
        protected override TokenBasedStorageEntry GetOrCreateStorageEntry()
        {
            return (TokenBasedStorageEntry)Storage.GetOrCreate(Key, _refillInterval, new TokenBasedStorageEntry
            {
                Tokens = _maxTokens,
                LastRefill = DateTime.UtcNow,
            });
        }

        protected override double CaculateRetryAfter(TokenBasedRateLimiterResult rateLimitResult)
        {
            if (rateLimitResult.StorageEntry != null)
            {
                var remainingTime = _refillInterval - (DateTime.UtcNow - rateLimitResult.StorageEntry.LastRefill);
                return Math.Max(remainingTime.TotalSeconds, 0);
            }

            return 0;
        }

        public override TokenBasedRateLimiterResult IsRequestAllowed()
        {
            //gets or create a new entry in the storage
            var storageEntry = GetOrCreateStorageEntry();

            var result = new TokenBasedRateLimiterResult
            { 
                StorageEntry = storageEntry 
            };

            var now = DateTime.UtcNow;
            var timeSinceLastRefill = now - storageEntry.LastRefill;
            var refillCount = (int)(timeSinceLastRefill.TotalSeconds / _refillInterval.TotalSeconds) * _tokensPerRefill;
            storageEntry.Tokens = Math.Min(storageEntry.Tokens + refillCount, _maxTokens);

            if (refillCount > 0)
            {
                storageEntry.LastRefill = now;
            }

            if(storageEntry.Tokens <= 0)
            {
                return result;
            }

            storageEntry.Tokens--;
            result.Success = true;
            Storage.Set(Key, _refillInterval, storageEntry);

            return result;
        }
    }
}
