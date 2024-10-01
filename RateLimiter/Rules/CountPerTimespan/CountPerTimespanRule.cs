using System;
using System.Threading.Tasks;
using RateLimiter.Common;
using RateLimiter.Stores;

namespace RateLimiter.Rules.CountPerTimespan
{
    public class CountPerTimespanRule : IRule
    {
        private readonly string name;
        private readonly ITimeProvider timeProvider;
        private readonly IRequestCountStore store;
        private readonly CountPerTimespanRuleOptions options;
        
        public CountPerTimespanRule(
            string name,
            ITimeProvider timeProvider,
            IRequestCountStore store,
            CountPerTimespanRuleOptions options)
        {
            this.name = name;
            this.timeProvider = timeProvider;
            this.store = store;
            this.options = options;
        }

        public string Name => this.name;

        public async Task<bool> Allow(Client client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            
            var requestSince = this.timeProvider.Now().Subtract(this.options.TimeSpan);

            long requestCountSince = await this.store.RequestCountSince(client.Id, requestSince);

            return requestCountSince >= this.options.MaxCount ? false : true;
        }
    }
}
