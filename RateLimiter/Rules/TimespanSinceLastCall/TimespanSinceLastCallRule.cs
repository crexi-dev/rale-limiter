using System;
using System.Threading.Tasks;
using RateLimiter.Common;
using RateLimiter.Stores;

namespace RateLimiter.Rules.TimespanSinceLastCall
{
    public class TimespanSinceLastCallRule : IRule
    {
        private readonly string name;
        private readonly ITimeProvider timeProvider;
        private readonly IRequestCountStore store;
        private readonly TimespanSinceLastCallRuleOptions options;

        public TimespanSinceLastCallRule(
            string name,
            ITimeProvider timeProvider, 
            IRequestCountStore store,
            TimespanSinceLastCallRuleOptions options)
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

            var now = this.timeProvider.Now();

            long requestCountSince = await this.store.RequestCountSince(client.Id, now.Subtract(this.options.TimeSpan));

            return requestCountSince > 0 ? false : true;
        }
    }
}
