using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using RateLimiter.Infrastructure;
using RateLimiter.Processors;
using RateLimiter.Rules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NuGet.Protocol;
using RateLimiter.Contracts;

namespace RateLimiter.Attributes;

/// The `BaseRulesProcessingAttribute` class is a base class that implements the `IAsyncResourceFilter` interface. It provides a set of base functionalities for processing rules based on a set of custom attributes.
/// This class is generic and can accept two type parameters (`TRuleResultType`, `TRuleContext`) that define the type of the rule result and the context in which the rules are processed.
/// The class contains the following members:
/// - `DefaultAttributeSettings`: A property of type `IRulesProcessingDefaults<TRuleResultType, TRuleContext>`, which represents the default attribute settings for the rules processing. It is virtual and can be overridden by derived classes.
/// The class constructor accepts the following parameters:
/// - `blockedSenders`: An instance of `ICacheRepository<string, DateTime, BlockedClientRecord>`, which represents a repository for storing blocked client records.
/// - `requestCache`: An instance of `ICacheRepository<string, RequestDetails, CachedRequestsRecord>`, which represents a repository for caching request details.
/// - `logger`: An instance of `ILogger<BaseRulesProcessingAttribute<AllowRequestResult, RequestDetails>>`, which is used for logging purposes.
/// - `processorFactory`: An instance of `IProcessorFactory`, which is responsible for creating rules processors.
/// - `contextExtender`: An instance of `IContextExtender`, which extends the execution context.
/// The class defines the following methods:
/// - `OnResourceExecutionAsync`: An asynchronous method that is called to execute the resource filter. It takes a `ResourceExecutingContext` parameter and a `ResourceExecutionDelegate` parameter. It executes the rules processing logic and calls the next delegate in the pipeline.
/// - `GetRulesProcessor`: An asynchronous method that returns a `IContextualRulesProcessor<IRateRule, AllowRequestResult, RequestDetails>` based on the provided `requestDetails` parameter. It retrieves the rules context from the request cache and creates a rules processor using the processor factory.
/// This class is meant to be used as a base class for implementing custom rules processing attributes.
/// /
public class BaseRulesProcessingAttribute<TRuleResultType, TRuleContext> : IAsyncResourceFilter
{
    private readonly ICacheRepository<string, DateTime, BlockedClientRecord> _blockedSenders;
    private readonly ICacheRepository<string, RequestDetails, CachedRequestsRecord> _requestCache;
    private IContextualRulesProcessor<IRateRule, AllowRequestResult, RequestDetails>? _rulesProcessor;
    private readonly ILogger<BaseRulesProcessingAttribute<AllowRequestResult, RequestDetails>> _logger;
    private double _blockExpires;
    private readonly IProcessorFactory _processorFactory;
    private readonly IContextExtender _contextExtender;

    public virtual IRulesProcessingDefaults<TRuleResultType, TRuleContext> DefaultAttributeSettings { get; set; }

    public BaseRulesProcessingAttribute(ICacheRepository<string, DateTime, BlockedClientRecord> blockedSenders,
        ICacheRepository<string, RequestDetails, CachedRequestsRecord> requestCache,
        ILogger<BaseRulesProcessingAttribute<AllowRequestResult, RequestDetails>> logger,
        IProcessorFactory processorFactory, IContextExtender contextExtender)
    {
        _blockedSenders = blockedSenders;
        _requestCache = requestCache;
        _logger = logger;
        _processorFactory = processorFactory;
        _contextExtender = contextExtender;
        SetBlockDeleteHandler();
    }

    /// <summary>
    /// This method sets the default values for the RateLimiterAttribute class.
    /// </summary>
    protected void SetDefaultValues()
    {
        _blockExpires = DefaultAttributeSettings?.DefaultBlockExpiresDuration ??
                        int.Parse(Environment.GetEnvironmentVariable("BLOCKEXPIRES_IN_SECONDS") ?? "15");
    }

    /// <summary>
    /// Sets the block delete handler for handling the deletion of blocked requests.
    /// </summary>
    private void SetBlockDeleteHandler()
    {
        var blocks = _blockedSenders as BlockedRequestsRepository;
        var requests = _requestCache as CachedRequestsRepository;
        blocks.RequestBlockDeleted += requests.HandleBlockDeleted;
    }

    /// <summary>
    /// This method performs resource execution async for the API Rate Limiter.
    /// </summary>
    /// <param name="context">The ResourceExecutingContext object that represents the context for the executing resource.</param>
    /// <param name="next">The ResourceExecutionDelegate object that represents the next resource execution in the pipeline.</param>
    /// <returns>Returns a Task object that represents the asynchronous execution of the method.</returns>
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
        ResourceExecutionDelegate next)
    {
        try
        {
            var requestDetails = await _contextExtender.CreateRequestDetailsFromContext(context.HttpContext.Request);

            if (await BlockRequestIfBlockedAlready(context, requestDetails)) return;

            await _requestCache.Update(requestDetails.RequestId, requestDetails);

            _rulesProcessor = await GetRulesProcessor(requestDetails);

            var shouldProcess = await _rulesProcessor.ProcessRules();

            if (!shouldProcess.AllowRequest)
            {
                var blockExpiresTime = DefaultAttributeSettings?.DefineBlockEndTime(DateTime.Now) ??
                                       DateTime.Now.AddSeconds(_blockExpires);
                await _blockedSenders.Add(requestDetails.RequestId, blockExpiresTime);
                await _requestCache.Delete(requestDetails.RequestId);
                SetContextResult(context, $"{blockExpiresTime:G}, Reason: {shouldProcess.Reason.ToUpper()} ");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            context.Result = new ContentResult
            {
                Content = "There was an error processing your request, please try again later".ToJson(),
                ContentType = "text/json", StatusCode = (int)HttpStatusCode.BadRequest
            };
            return;
        }

        await next();
    }

    /// <summary>
    /// Checks if a request is blocked and sets the appropriate context result if it is.
    /// </summary>
    /// <param name="context">The resource executing context.</param>
    /// <param name="requestDetails">The request details.</param>
    /// <returns>True if the request is blocked, false otherwise.</returns>
    private async Task<bool> BlockRequestIfBlockedAlready(ResourceExecutingContext context,
        RequestDetails requestDetails)
    {
        try
        {
            var blockRequest = await CheckIfRequestIsBlocked(requestDetails.RequestId);

            if (blockRequest != null)
            {
                SetContextResult(context, blockRequest.BlockExpires.ToString("G"));
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    /// <summary>
    /// Retrieves the rules processor for the given request details.
    /// </summary>
    /// <param name="requestDetails">The request details for which to retrieve the rules processor.</param>
    /// <returns>The rules processor for the given request details.</returns>
    public async Task<IContextualRulesProcessor<IRateRule, AllowRequestResult, RequestDetails>> GetRulesProcessor(
        RequestDetails requestDetails)
    {
        try
        {
            var rulesContext = await _requestCache.GetMany(requestDetails.RequestId);

            var defaultRules = await SetDefaultRules(requestDetails);
            
            _rulesProcessor =
                await _processorFactory.GetContextualProcessor<AllowRequestResult, RequestDetails>(defaultRules,
                    rulesContext);

            if (_rulesProcessor == null)
            {
                _logger.LogError("PROCESSOR NOT CREATED", [defaultRules, rulesContext]);
                throw new NullReferenceException("PROCESSOR NOT CREATED");
            }

            return _rulesProcessor;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    /// <summary>
    /// Sets the default rules for rate limiting processing.
    /// </summary>
    /// <param name="details">The details of the request.</param>
    /// <returns>The collection of default rate rules.</returns>
    private async Task<IEnumerable<IRateRule>> SetDefaultRules(RequestDetails details)
    {
        return await Task.Run(() =>
        {
            try
            {
                var baseDefaultRules = (IEnumerable<IRateRule>)DefaultAttributeSettings?.DefaultRules ?? null;
                if (DefaultAttributeSettings?.DefaultBehavior == RuleProcessingBehaviors.Override)
                    return baseDefaultRules;
                var rules = _processorFactory.GetDefaultRuleset(details.Location) ?? [];
                if (baseDefaultRules != null &&
                    DefaultAttributeSettings?.DefaultBehavior == RuleProcessingBehaviors.Add)
                    rules.AddRange(baseDefaultRules);

                rules.Add(new TrueRule("added from attribute"));

                var firstRule = new TrueRule("first rule for AndRule");
                var trueRule = new TrueRule("second rule for AndRule");
                var falseRule = new FalseRule("second rule for AndRule");

                rules.Add(new AndRule(firstRule, trueRule));
                rules.Add(new OrRule(falseRule, trueRule));

                var complexRule = new IfThenRuleBuilder();

                Func<RequestDetails, bool> thing = (rd =>
                {
                    var day = rd.RequestTime.DayOfWeek;
                    return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
                });

                Func<IEnumerable<RequestDetails>, Task<AllowRequestResult>> blarg = (async rd =>
                {
                    return new AllowRequestResult(true, "It's the Weekend!");
                });

                var ifThen = complexRule.WithRule((rd =>
                    {
                        var day = rd.RequestTime.DayOfWeek;
                        return day is DayOfWeek.Saturday or DayOfWeek.Sunday;
                    }), (async rd =>
                    {
                        Console.WriteLine("It's the weekend!");
                        return new AllowRequestResult(true, "It's the Weekend!");
                    }), details)
                    .WithRule((rd =>
                    {
                        var day = rd.RequestTime.DayOfWeek;
                        return day is not DayOfWeek.Saturday or DayOfWeek.Sunday;
                    }), (async rd =>
                    {
                        Console.WriteLine("Back to work!");
                        return new AllowRequestResult(true, "Back to work!");
                    }), details)
                    .Build();

                rules.Add(ifThen);
                rules.Add(new RequestsOverTimespanRule());
                return rules;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        });
    }

    /// <summary>
    /// Performs rules processing for a resource.
    /// </summary>
    /// <typeparam name="TRuleResultType">The type of the rule processing result.</typeparam>
    /// <typeparam name="TRuleContext">The type of the rule processing context.</typeparam>
    private static void SetContextResult(ResourceExecutingContext context, string message)
    {
        try
        {
            var blockedResult = new ContentResult
            {
                Content = $"Requests to this resource are blocked until {message}".ToJson(), ContentType = "text/json",
                StatusCode = (int)HttpStatusCode.BadRequest
            };

            context.Result = blockedResult;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    /// Checks if a request is blocked based on the request details.
    /// @param requestId The unique identifier of the request.
    /// @return A Task that represents the asynchronous operation. The task result contains a BlockedClientRecord object
    /// if the request is blocked, or null if the request is not blocked.
    /// /
    private async Task<BlockedClientRecord> CheckIfRequestIsBlocked(string requestId)
    {
        try
        {
            return await _blockedSenders.Get(requestId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }
}