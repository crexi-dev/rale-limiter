
using RateLimiter.Services;
using RateLimiter.Models;
using RateLimiter.Dto;
using RateLimiter.Repositories;
using RateLimiter.Tests.Fixture;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Threading;

namespace RateLimiter.Tests
{
    public class RateLimiterTest : IClassFixture<ApplicationDBFixture>
    {
        private readonly IResourceService _resourceService;
        private readonly IRuleService _ruleService;
        private readonly IResourceRuleService _resourceRuleService;
        private readonly IResourceVisitLogService _resourceVisitLogService;
        private ApplicationDBFixture _applicationDBFixture;

        public RateLimiterTest(ApplicationDBFixture dbFixture)
        {
            this._resourceService = new ResourceService(new ResourceRepository(dbFixture.DBContext));
            this._ruleService = new RuleService(new RuleRepository(dbFixture.DBContext));
            this._resourceRuleService = new ResourceRuleService(new ResourceRuleRepository(dbFixture.DBContext));
            this._resourceVisitLogService = new ResourceVisitLogService(new ResourceVisitLogRepository(dbFixture.DBContext));
        }

        [Fact]
        public async Task AllowAPIRequest_Test_Pass1()
        {
            AccessToken token = new Dto.AccessToken()
            {
                EndpointUrl = "www.cnn.com",
                SessionId = "abcdedf"
            };

            IRateLimiterService service = new RateLimiterService(this._resourceVisitLogService, this._resourceRuleService);
            var allowed = await service.AllowAPIRequest(token);
            Assert.True(allowed);
        }

        [Fact]
        public async Task AllowAPIRequest_Test_Fail1()
        {
            AccessToken token = new Dto.AccessToken()
            {
                EndpointUrl = "www.bbc.com",
                SessionId = "xyz123"
            };

            await this._resourceVisitLogService.AddResourceVisitLog(new ResourceVisitLog()
            {
                ResourceId = 2,
                SessionId = token.SessionId,
                visitTime = DateTime.UtcNow
            });

            Thread.Sleep(3000);
            IRateLimiterService service = new RateLimiterService(this._resourceVisitLogService, this._resourceRuleService);
            var allowed = await service.AllowAPIRequest(token);

            Assert.False(allowed);
        }

        [Fact]
        public async Task AllowAPIRequest_Test_Pass2()
        {
            AccessToken token = new Dto.AccessToken()
            {
                EndpointUrl = "www.netflix.com",
                SessionId = "xyz123"
            };

            for (int i = 1; i <= 3; i++)
            {
                await this._resourceVisitLogService.AddResourceVisitLog(new ResourceVisitLog()
                {
                    ResourceId = 3,
                    SessionId = token.SessionId,
                    visitTime = DateTime.UtcNow
                });
                Thread.Sleep(5000);
            }

            IRateLimiterService service = new RateLimiterService(this._resourceVisitLogService, this._resourceRuleService);
            var allowed = await service.AllowAPIRequest(token);

            Assert.True(allowed);
        }

        [Fact]
        public async Task AllowAPIRequest_Test_Fail2()
        {
            AccessToken token = new Dto.AccessToken()
            {
                EndpointUrl = "www.netflix.com",
                SessionId = "xyz123"
            };

            for (int i = 1; i <= 3; i++)
            {
                await this._resourceVisitLogService.AddResourceVisitLog(new ResourceVisitLog()
                {
                    ResourceId = 3,
                    SessionId = token.SessionId,
                    visitTime = DateTime.UtcNow
                });
                Thread.Sleep(3000);
            }

            IRateLimiterService service = new RateLimiterService(this._resourceVisitLogService, this._resourceRuleService);
            var allowed = await service.AllowAPIRequest(token);

            Assert.False(allowed);
        }
    }
}