using Microsoft.Extensions.DependencyInjection;
using RateLimiter.Common;
using RateLimiter.Counter;
using RateLimiter.Rules;
using RateLimiter.Stores;
using System;
using System.Threading.Channels;

namespace RateLimiter
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRateLimiter(this IServiceCollection services)
        {
            var channel = Channel.CreateBounded<NewRequest>(new BoundedChannelOptions(1000000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = true,
            });

            services.AddSingleton(channel.Reader);
            services.AddSingleton(channel.Writer);
            services.AddSingleton<IRequestCounter, RequestCounter>();
            services.AddSingleton<IRequestCountStore, InMemoryRequestCountStore>();
            services.AddSingleton<ITimeProvider, SystemTimeProvider>();
            services.AddSingleton<IRuleService, RuleService>();
        }
    }
}
