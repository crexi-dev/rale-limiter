using RateLimiter.Models;
using System;
using System.Collections.Generic;

namespace RateLimiter.Repositories
{
    public static class RequestsData
    {
        private static Dictionary<string, List<RequestModel>> Requests { get; set; } = new();

        public static void SaveRequests(string resource, string token, DateTime requestTime)
        {
            List<RequestModel> requests;

            if (Requests.TryGetValue(token, out List<RequestModel>? value))
            {
                requests = value;
            }
            else
            {
                requests = new List<RequestModel>();
            }

            var request = new RequestModel
            {
                Resource = resource,
                RequestTime = requestTime
            };

            requests.Add(request);
            Requests[token] = requests;
        }

        public static IEnumerable<RequestModel>? GetRequests(string token)
        {
            if (Requests.TryGetValue(token, out List<RequestModel>? value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
    }
}