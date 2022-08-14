﻿using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class RateLimiter : DelegatingHandler
    {
        //TODO: Could be replaced by a HashSet since key and resource key are the same
        private readonly Dictionary<string, Resource> _resources;
        int retry_seconds = 0;

        /// <summary>
        /// Obtain the Resource Id based on request
        /// default is request AbsolutePath, you can replace
        /// for more complex ids like: 
        ///  - "POST:/login"
        ///  - "GET:/book?id=1"
        /// </summary>
        public Func<HttpRequestMessage, string> GetResourceId { get; set; } = (request) =>
        {
            return request.RequestUri.AbsolutePath;
        };

        public RateLimiter(Dictionary<string, Resource> resources) : base(new HttpClientHandler())
        {
            _resources = resources;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (GetResourceId == null)
            {
                throw new Exception(nameof(GetResourceId) + ":Cannot be null");
            }

            var id = GetResourceId(request);
            var canAccess = false;
            
            if (_resources.ContainsKey(id))
            {
                var resource = _resources[id];
                var rules = resource.Rules;
                canAccess = CanAccess(rules, request);
            }
            if (canAccess)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.TooManyRequests);
            if (retry_seconds != 0)
            {
                response.Headers.Add(HeaderNames.RetryAfter, retry_seconds.ToString(NumberFormatInfo.InvariantInfo));
            }
            retry_seconds = 0;
            return response;
        }

        private bool CanAccess(List<RateLimiterRule> rules, HttpRequestMessage request)
        {
            if (rules == null)
                return true;
            
            if (rules.Count == 0)
                return true;

            var canAccess = true;
            
            //TODO: Depending of amount rules/performance requirements this could be replaced for a parallel for-loop            
            foreach (var item in rules)
            {
                var isValid = item.IsValid(request);                
                canAccess = canAccess && isValid;
                retry_seconds = Math.Max(item.RetryAfterSeconds, retry_seconds);

                // exiting without processing anymore rules: access was denied.
                if (canAccess == false)
                    break;
            }
            return canAccess;
        }

    }
}
