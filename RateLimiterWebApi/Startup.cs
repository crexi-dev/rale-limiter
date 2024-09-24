using FluentValidation;
using Newtonsoft.Json;
using RateLimiter.Interfaces;
using RateLimiter.Models.Apis;
using RateLimiter.Models.Entities;
using RateLimiter.Models.RatePolicies;
using RateLimiter.Persistent;
using RateLimiter.Repositories;
using RateLimiter.Services;
using RateLimiter.Validators;
using System.Net.NetworkInformation;

namespace RateLimiterWebApi
{
    /// <summary>
    /// Class to register all classes used in the sample webapi.
    /// </summary>
    public static class Startup
    {
        public static void ConfigServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<PersistentContext>(_ => new PersistentContext()
            {
                Users = GetInitialMockUsers(),
                Resources = GetInitialMockResources(),
                Policies = GetInitialMockPolicies(),
                UserResourcePolicies = GetInitialUserResourcePolicies(),
                UserActivities = new List<UserActivity>(),
            });

            services.AddSingleton<IValidator<RateLimitRequest>, RateLimitRequestValidator>(_ => new RateLimitRequestValidator());
            services.AddTransient<IPersistentProvider, MemoryPersistentProvider>();
            services.AddTransient<IPolicyVerifier, MemoryPersistentProvider>();

            services.AddTransient<IPolicyRepository, PolicyRepository>();
            services.AddTransient<IResourceRepository, ResourceRepository>();
            services.AddTransient<IUserRepository, UserRepository>();

            services.AddTransient<IPolicyService, PolicyService>();
            services.AddTransient<IRateLimitService, RateLimitService>();
        }

        private static List<User> GetInitialMockUsers()
        {
            return new List<User>()
            {
                new User()
                {
                    UserId = "u1",
                    DisplayName = "User1",
                },
                new User()
                {
                    UserId = "u2",
                    DisplayName = "User2",
                },
                new User()
                {
                    UserId = "u3",
                    DisplayName = "User3",
                },
            };
        }

        private static List<Resource> GetInitialMockResources()
        {
            return new List<Resource>()
            {
                new Resource()
                {
                    ResourceId = "r1",
                    Content = "Resource1",
                },
                new Resource()
                {
                    ResourceId = "r2",
                    Content = "Resource2",
                },
            };
        }

        private static List<Policy> GetInitialMockPolicies()
        {
            var timeSpanInfo = new TimeSpanInfo()
            {
                SpanInSeconds = 60,
                MaxCalls = 5,
            };

            var lastCallInfo = new LastCallInfo()
            {
                MinLastCallSeconds = 10,
            };

            return new List<Policy>()
            {
                new Policy()
                {
                    PolicyId = "p1",
                    PolicyName = "NoLimit",
                    PolicyJson = "{}"
                },
                new Policy()
                {
                    PolicyId = "p2",
                    PolicyName = "TimeSpan",
                    PolicyJson = JsonConvert.SerializeObject(timeSpanInfo),
                },
                new Policy()
                {
                    PolicyId = "p3",
                    PolicyName = "LastCall",
                    PolicyJson = JsonConvert.SerializeObject(lastCallInfo),
                },
            };
        }

        private static List<UserResourcePolicy> GetInitialUserResourcePolicies()
        {
            return new List<UserResourcePolicy>()
            {
                new UserResourcePolicy()
                {
                    UserId = "u1",
                    ResourceId = "r1",
                    Policies = new List<string>{ "p1" },
                },
                new UserResourcePolicy()
                {
                    UserId = "u2",
                    ResourceId = "r1",
                    Policies = new List<string>{ "p2" },
                },
                new UserResourcePolicy()
                {
                    UserId = "u3",
                    ResourceId = "r1",
                    Policies = new List<string>{ "p3" },
                },
                new UserResourcePolicy()
                {
                    UserId = "u1",
                    ResourceId = "r2",
                    Policies = new List<string>{ "p2", "p3" },
                },
            };
        }
    }
}
