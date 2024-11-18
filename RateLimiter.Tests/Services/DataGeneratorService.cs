using NUnit.Framework;
using RateLimiter.Data.Models;
using RateLimiter.Tests.Interfaces;
using System;
using System.Collections.Generic;

namespace RateLimiter.Tests.Services
{
    internal class DataGeneratorService : IDataGeneratorService
    {
        public Request GenerateRequest(int id, Resource resource, User user, string identifier, bool? wasHandled)
        {
            var request = new Request
            {
                Id = id,
                Identifier = identifier,
                RequestDate = DateTime.Now,
                ResourceId = resource.Id,
                Resource = resource,
                UserId = user.Id,
                User = user,
                WasHandled = wasHandled,
                CreatedBy = "DataGenerator",
                CreatedDate = DateTime.Now
            };

            return request;
        }
        public Resource GenerateResource(int id, string name, Status status, List<LimiterRule> limiterRules)
        {
            var resource = new Resource
            {
                Id = id,
                Identifier = name,
                Name = name,
                Description = name,
                Status = status,
                LimiterRules = limiterRules,
                CreatedBy = "DataGenerator",
                CreatedDate = DateTime.Now
            };

            return resource;
        }
        public User GenerateUser(int id, string username, Guid token, string tokenSource)
        {
            var user = new User
            {
                Id = id,
                Identifier = token.ToString(),  // small hack - Find() only works with attributes in the BaseModel so user token must reside in the Identifier for now.
                Name = username,  
                Token = token.ToString(),
                TokenSource = tokenSource,
                Email = string.Format("{0}@phonyEmail.com", username),
                CreatedBy = "DataGenerator",
                CreatedDate = DateTime.Now
            };

            return user;
        }
        public LimiterRule GenerateLimiterRule(int id, string name, string? tokenSource, int? resourceStatusId, int numPerTimespan, int numSeconds)
        {
            var limiterRule = new LimiterRule
            {
                Id = id ,
                Identifier = name ,
                Name = name ,
                TokenSource = tokenSource ,
                ResourceStatusId = resourceStatusId ,
                NumPerTimespan = numPerTimespan ,
                NumSeconds = numSeconds,
                CreatedBy = "DataGenerator",
                CreatedDate = DateTime.Now
            };

            return limiterRule;
        }
    }
}
