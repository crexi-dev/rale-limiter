using RateLimiter.Providers;
using RateLimiter.Store;

namespace RateLimiter.Rules
{
    public class MinimumTimeIntervalRule : IRateLimiterRule
    {
        private readonly IDataStore _dataStore;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly int _minTimeIntervalInSeconds;

        public MinimumTimeIntervalRule(IDataStore dataStore, IDateTimeProvider dateTimeProvider, int minTimeIntervalInSeconds)
        {
            _dataStore = dataStore;
            _dateTimeProvider = dateTimeProvider;
            _minTimeIntervalInSeconds = minTimeIntervalInSeconds;
        }

        public bool IsAllowed(string token, string uri)
        {
            var requstTimestamp = _dateTimeProvider.UtcNow;

            var lastRequest = _dataStore.GetLastRequestByClient(token);

            _dataStore.AddClientRequest(token, uri);

            return lastRequest == null ||
                (requstTimestamp - lastRequest.Timestamp).TotalSeconds > _minTimeIntervalInSeconds;
        }
    }
}
