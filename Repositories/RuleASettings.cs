using RateLimiter.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Repositories
{
    public static class RuleASettings
    {
        private static ConcurrentDictionary<string, RuleAModel> Settings { get; set; } = new();

        public static RuleAModel GetSetting(string resource)
        {
            if (Settings.TryGetValue(resource, out RuleAModel? value))
            {
                return value;
            }

            throw new InvalidOperationException($"No rules set for resource {resource}");
        }

        public static void SaveSetting(string resource, int requests, int timeSpanSecs)
        {
            var settings = new RuleAModel
            {
                Requests = requests,
                TimeSpanSecs = timeSpanSecs
            };

            Settings[resource] = settings;
        }
    }
}