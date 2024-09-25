using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RateLimiter.Contracts;
using RateLimiter.Rules;

namespace RateLimiter.Processors;

public class RateRulesProcessor : IContextualRulesProcessor<IRateRule, AllowRequestResult, RequestDetails>
{
    public RateRulesProcessor(IEnumerable<IRateRule> rules,
        IContextualProcessingEngine<IRateRule, AllowRequestResult, RequestDetails> processingEngine,
        IEnumerable<RequestDetails>? context)
    {
        Ruleset = new RulesCollection<IRateRule>
        {
            Rules = rules
        };
        Engine = processingEngine;
        Context = context;
    }

    protected RateRulesProcessor()
    {
    }

    public IContextualProcessingEngine<IRateRule, AllowRequestResult, RequestDetails> Engine { get; set; }
    public RulesCollection<IRateRule>? Ruleset { get; set; }

    public IEnumerable<RequestDetails>? Context { get; set; }

    public async Task<AllowRequestResult> ProcessRules()
    {
        ValidateProcessingInputs();
        return await Engine.ProcessRules(Ruleset.Rules, Context);
    }

    private void ValidateProcessingInputs()
    {
        ArgumentNullException.ThrowIfNull(Ruleset);
        ArgumentNullException.ThrowIfNull(Ruleset.Rules);
        ArgumentNullException.ThrowIfNull(Context);
        ArgumentNullException.ThrowIfNull(Engine);
    }
}