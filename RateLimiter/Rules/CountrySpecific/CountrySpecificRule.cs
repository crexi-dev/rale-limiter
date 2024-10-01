using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateLimiter.Common;

namespace RateLimiter.Rules.CountrySpecific
{
    public class CountrySpecificRule : IRule
    {
        private string name;
        private readonly IDictionary<string, IEnumerable<IRule>> mapping;

        public CountrySpecificRule(
            string name,
            IDictionary<string, IEnumerable<IRule>> mapping)
        {
            this.name = name;
            this.mapping = mapping;
        }

        public string Name => this.name;

        public async Task<bool> Allow(Client client)
        {
            if (!this.mapping.ContainsKey(client.CountryCode))
            {
                return true;
            }

            List<Task<bool>> tasks = new();
            var rules = this.mapping[client.CountryCode];

            foreach (var rule in rules)
            {
                tasks.Add(rule.Allow(client));
            }

            await Task.WhenAll(tasks);

            return tasks.All(t => t.Result);
        }
    }
}
