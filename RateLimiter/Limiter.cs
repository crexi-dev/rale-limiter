using Microsoft.Extensions.Configuration;
using RateLimiter.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace RateLimiter
{
    public class Limiter
    {
        #region - Fields and Props  - 
        private static readonly Lazy<Limiter> _lazyInstance = new Lazy<Limiter>(() => new Limiter());
        private static readonly LimiterConfig _config;
        private static readonly ConcurrentDictionary<string, RateLimit> _hits = new ConcurrentDictionary<string, RateLimit>();


        public static Limiter Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        public LimiterConfig Config { get => _config; } 
        #endregion

        private Limiter() { } // Private constructor

        static Limiter()
        {
            Console.WriteLine($"Loading configuration");

            try
            {
               

                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                _config = new LimiterConfig();
                configuration.GetSection("limiter").Bind(_config);

                var sectioncount = configuration.GetSection("limiter");


                if (_config == null)
                {
                    throw new InvalidOperationException("Failed to load limiter configuration.");
                }

                Console.WriteLine($"Loaded {_config.Rules.Count} rate limiting rules.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                throw; // Re-throw the exception to prevent the class from being used with invalid configuration
            }
        }


        public LimitResult ApplyRateLimit(string userToken, string apiUrl, IDictionary<string, string> headers, LimiterConfig? config = null)
        {
            Console.WriteLine($"\n---- ApplyRateLimit Invoked ----\n");

            config = config ?? _config;

            LimitResult? result = null;
            Rule? defaultRule = null;
            DateTime now = DateTime.UtcNow;

            
            foreach (Rule item in config.Rules)
            {
                if (item.IsDefault)
                {
                    defaultRule = item;
                    continue;
                }

                bool isMatch = HasMatchingUrl(apiUrl, item);
                if (isMatch && AreConditionsMatching(headers,item) )
                {
                    Console.WriteLine($"\n\tMatched {userToken} with '{item.Name}' for {apiUrl}\n");

                    result = ProcessRule(userToken, apiUrl, item, now);

                    if (result != LimitResult.Success)
                    {
                        //no need to check any other Rules if we already know we have to reject
                        break;
                    }
                }
                else
                {
                    Console.WriteLine($"\n\tNo Match {userToken} with '{item.Name}' for {apiUrl}\n");
                }
            }

            //if (defaultRule != null && (result == null || result == LimitResult.Success) )
            if (defaultRule != null && (result == null))
            {
                result = ProcessRule(userToken, apiUrl, defaultRule, now);

            }

            if (result == null)
            {
                result = LimitResult.Unmatched;
                Console.WriteLine($"\tLimitResult.Unmatched is set");
            }

            Console.WriteLine($"\n---- Limit results.IsSuccessful: {result.IsSuccessful} ----");

            return result;
        }

        private static bool HasMatchingUrl(string apiUrl, Rule item)
        {
            return Regex.IsMatch(apiUrl, item?.Match?.ApiUrl);
        }

        private static bool AreConditionsMatching(IDictionary<string, string> headers, Rule item)
        {
            bool? requestMatches = null;
            int conditionCount = item.Conditions?.Count ?? 0;

            if (conditionCount == 0)
            {
                requestMatches = true;
                Console.WriteLine($"\t\tNo Conditions to restrict Matching");
            }
            else if(headers == null || headers.Count == 0)
            {
                //if we have conditions but no headers, then we cannot meet our conditions
                requestMatches = false;
                Console.WriteLine($"\t\tNo Headers to qualify against. No Match.");
            }
            else
            {   //we have conditions and headers to check against

                foreach (var condition in item.Conditions)
                {
                    Console.WriteLine($"\n\t\tChecking Condition {condition.Input}");
                    //if any condition is missing or Not matching we disqualify
                    if (!headers.ContainsKey(condition.Input) ||  !Regex.IsMatch(headers[condition.Input], condition.Pattern))
                    {
                        requestMatches = false;
                        Console.WriteLine($"\t\tCondition Disqualified {condition.Input}");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"\t\t\tCondition Passed {condition.Input} {headers[condition.Input]}, {condition.Pattern}");
                    }
                }
                if(!requestMatches.HasValue)
                {
                    requestMatches = true;
                }
                
            }

            return requestMatches.Value;
        }

        private LimitResult? ProcessRule(string userToken, string apiUrl, Rule currentRule, DateTime now)
        {
            LimitResult? result = null;

            for (int idx = 0; idx < currentRule.Limits.Count; idx++)
            {
                Limit limit = currentRule.Limits[idx];

                Console.WriteLine($"\n\tEvaluating Limit type {limit.Type.ToString("G")} of Rule: '{currentRule.Name}' ");

                if (limit.Type == LimitType.TimeWindow)
                {
                    result = CheckHitLimit(userToken, apiUrl, currentRule, now, idx, limit);
                    if (result != LimitResult.Success)
                    {
                        break;
                    }
                }
                else if (limit.Type == LimitType.RequestSpacing)
                {
                    result = CheckSpacingLimit(userToken, apiUrl, currentRule, now, limit);

                    if (result != LimitResult.Success)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine($"\n\t\tProcessRule Evaluation yields IsSuccessful: {result.IsSuccessful},  LimitResult.Success == {result == LimitResult.Success}");

            return result;
        }

        private LimitResult CheckHitLimit(string userToken, string apiUrl, Rule defaultRule, DateTime now, int idx, Limit limit)
        {
            LimitResult? result = null;

            RateLimit userLimit = GetUserRateLimit(userToken, apiUrl, now, limit.WindowType);


            if (now.GetRateValue(limit.WindowType) != userLimit.FirstHit.GetRateValue(limit.WindowType))
            {
                Console.WriteLine($"\tResetting Count for {idx} of {defaultRule.Name}");
                userLimit.HitCount = 0;
                userLimit.FirstHit = now;
            }
            else
            {
                userLimit.HitCount++;
            }

            string stats = $"Now's Val: {now.GetRateValue(limit.WindowType)} , FirstHit Val: {userLimit.FirstHit.GetRateValue(limit.WindowType)}";

            if (userLimit.HitCount > limit.RequestLimit)
            {
            
                result = new LimitResult
                {
                    IsSuccessful = false
                    ,
                    FailReason = $"Rate limit exceeded for {userToken} on {apiUrl} with RequestLimit: {limit.RequestLimit} for {limit.WindowType}, {stats}"
                    ,
                    Rule = defaultRule
                };
                
                Console.WriteLine($"\t*** CheckHitLimit IsSuccessful = false, {limit.WindowType}, HitCount {userLimit.HitCount} , limit.RequestLimit:{limit.RequestLimit}, {stats}");
            }
            else
            {
                Console.WriteLine($"\tCheckHitLimit LimitResult.Success {limit.WindowType}, HitCount {userLimit.HitCount} , limit.RequestLimit:{limit.RequestLimit}, , {stats}");
                result = LimitResult.Success;
                result.Rule = defaultRule;
            }            

            return result;
        }

        private LimitResult CheckSpacingLimit(string userToken, string apiUrl, Rule currentRule, DateTime now, Limit limit)
        {
            LimitResult? result = null;

            RateLimit userLimit = GetUserRateLimit(userToken, apiUrl, now);
            TimeSpan spacingRequired = limit.Spacing;
            TimeSpan timeBetweenRequests = now-userLimit.FirstHit;

            string msg = $"Time Spacing required: {spacingRequired.TotalMilliseconds} seconds" +
                                         $", Actual time passed {timeBetweenRequests.TotalMilliseconds} seconds have passed.";

            //update regardless b/c if they keep trying to hit when they should not, it should still count against them
            userLimit.FirstHit = now;
            userLimit.HitCount++;

            if (userLimit.HitCount == 1 || timeBetweenRequests >= spacingRequired)
            {
                Console.WriteLine("\t----\tResetting time:\t"+msg);
                userLimit.HitCount++;
                result = LimitResult.Success;
                result.Rule = currentRule;
            }
            else
            {               
                Console.WriteLine("\t****\tFailed: " + msg);
                TimeSpan waitTime = spacingRequired - timeBetweenRequests;

                result = new LimitResult
                {
                    IsSuccessful = false,
                    FailReason = $"Rate limit exceeded for {userToken} on {apiUrl}. " +
                                 $"Time Spacing required: {spacingRequired.TotalMilliseconds:F2} seconds, " +
                                 $"but only {timeBetweenRequests.TotalMilliseconds:F2} seconds have passed. " +
                                 $"Please wait {waitTime.TotalMilliseconds:F2} seconds before trying again.",
                    Rule = currentRule
                };
            }

            return result;
        }

        private RateLimit GetUserRateLimit(string userToken, string apiUrl, DateTime nowTime, TimeWindowType window)
        {
            string key = $"{userToken}_{window.ToString("G")}_{apiUrl.ToLower().Trim()}";

            return _hits.GetOrAdd(key, _ => new RateLimit { FirstHit = nowTime });            
        }

        private RateLimit GetUserRateLimit(string userToken, string apiUrl, DateTime nowTime)
        {
            string key = $"{userToken}_{apiUrl.ToLower().Trim()}";

            return _hits.GetOrAdd(key, _ => new RateLimit { FirstHit = nowTime });
        }



    }
}
