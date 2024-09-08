using RateLimiter.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RateLimiter.Repositories
{
    public static class RuleBSettings
    {
        private static ConcurrentDictionary<string, RuleBModel> Settings { get; set; } = new();

        public static RuleBModel GetSetting(string resource)
        {
            if (Settings.TryGetValue(resource, out RuleBModel? value))
            {
                return value;
            }

            throw new InvalidOperationException($"No rules set for resource {resource}");
        }

        public static void SaveSetting(string resource, int timeSpanSecs)
        {
            var settings = new RuleBModel
            {
                TimeSpanSecs = timeSpanSecs
            };

            Settings[resource] = settings;
        }
    }
}