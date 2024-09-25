# API Rate Limiter

## Description
This class library enables implementation of rate-limiting an http resource via attributes.

Different behaviors can be defined at the class or method level by using different attributes, and the same
attribute can be implemented multiple times with different behavior by using custom options.

The workflow is as follows:
- Incoming requests to a resource are used to generate a _RequestDetails_ object.  
  - RequestDetails has an identifier created by a hash of the client token and resource path, as well as capturing other metadata from the request.  This is the id that will 
    be used to compare requests over time, and to establish temporary blocks of requests
- The attribute will check if there is a current block for the incoming request.  If so, further processing stops and a message is returned 
    identifying when the block will end, and also why it was created
- If there is not a current block, the request will be saved in a cache, and processed by a *Processor*, dynamically generated at runtime and comprising a set of 
    rules, and a rule-processing engine
  - The processor engine will process all rules, using the current request and prior requests with the same RequestID - defined internally as a *context* - and after processing them, return an *AllowRequestResult* that defines whether the current request
      will be allowed to continue or not.  NOTE: The default engine that is defined uses a short-circuit to stop further processing of rules
      if a running rule returns false.  This improves efficiency, but also reveals the bias built in to this system --- 
  
      ```
        Rules in the RateLimiter domain return a boolean result, and should be created such that 'true' means continue processing, and 'false' means stop processing.
      ```
  - This is supported by the *AllowRequestResult* contract - the result is provided with an 'AllowRequest' property.  This was done to try and moderate the ambiguity that can be created in scenarios like this.
- If the engine returns 'false', a block is created and stored in a cache, to prevent future requests from that client to the same resource in the future.
- If the engine returns 'true', the request goes forward for further processing.
### Processors
The work of the attribute is accomplished through a Processor.  A Processor will be unique by it's collection
of rules, and the engine it uses to process the rules.  

### Rules
Accordingly, a ruleset is defined as a collection
of identical rule types, which are defined by the type the rule accepts as a *context*, and the type returned
by the rule as a result.

#### Defined Rules
The library includes the following concrete rules for use:
- And Rule: Compares two child rules and returns a result if both rules return the same result 
- Or Rule: Compares two child rules and returns a result if either rule returns a result as defined in the Or rule logic
- IfThen Rule: Allows definition of a set of condition/logic pairs, i.e. if the condition is met, the logic will be exectuted. This allows for modeling of very complex logic scenarios
- RequestsOverTimespan Rule: Provided as a basic implementation of a rule in the rate limiting domain.  If the number of requests in a given timespan exceeds a predefined number, a block is issued
- TimeBetweenRequests Rule: Provided as a basic implementation of a rule in the rate limiting domain.  If the time between requests to the same resource exceeds a specified number, a block is issued
- Base Rule: Provided as a wrapper for custom logic either defined separately or dynamically.  
- Boolean, True, False Rules: Provided as utility rules for testing or use in default ruleset scenarios

#### Rulesets
Rules are composed into *Rulesets*. Engines process rulesets to identify whether a request should be blocked.  Several default rulesets are provided
as examples of how different sets of logic can be created for different conditions, namely, defining default, distinct sets
of rules depending on the region a request comes from.

Custom rulesets can also be provided at the attribute scope, and defined as a base ruleset to which others rules can be 
added, or as the complete ruleset to be used for that attribute class.

- Note: There are several helper methods provided to allow consumers to generate rules ad-hoc in flow as required.

#### Engines
A *DefaultEngine* class is defined that performs the actions described so far.  An *EngineFactory* provides the engine for the processor, and allows for custom logic
to be implemented to conditionally generate a different engine if required. This would comprise changing the operations defined in the 'ProcessRules' method.  For example,
for especially high-security scenarios, if a rule fails, notifications could be generated to further flag the requestor, etc.

# Implementation
- The library as provided presumes a *token* header is provided on every request, as well as a location identifier.
- Rate-limiting behavior is applied to a resource (controller/action) via an attribute:
```csharp
[ServiceFilter(typeof(RateLimiterAttribute))]
```
- This attribute defines the use of the RateLimiterAttribute to manage the resource.
- Multiple attributes can be used together to provide custom request management, for example, defining 
a separate attribute for different controllers based on security profiles, etc.
- Attribute behavior is defined with the __BaseRuleProcessingAttribute__ class. *Note: Because of how middleware is implemented, this class CANNOT be abstract*
- Applying an attribute requires extending the base processor, as done in the RateLimiterAttribute class:
```csharp
public class RateLimiterAttribute : BaseRulesProcessingAttribute<AllowRequestResult, RequestDetails>
{
    
    public RateLimiterAttribute(ICacheRepository<string, DateTime, BlockedClientRecord> blockedSenders, ICacheRepository<string, RequestDetails, CachedRequestsRecord> requestCache, ILogger<BaseRulesProcessingAttribute<AllowRequestResult,RequestDetails>> logger, IProcessorFactory processorFactory, IContextExtender contextExtender) : base(blockedSenders, requestCache, logger, processorFactory, contextExtender)
    {
        DefaultAttributeSettings = new RateLimiterDefaults();
        SetDefaultValues();
    }
}
```
- A **DefaultAttributeSettings** property can be set with a new *Defaults class.  By using a defaults class, you can define
a custom set of rules that are defined at the attribute scope, as well as default block durations, and custom logic to define how long
a block should last.  Necessarily, the properties of default objects are subject to pre-definition for this specific domain, but the approach can be customized as required.