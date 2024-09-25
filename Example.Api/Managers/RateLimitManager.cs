using System.Collections.Concurrent;
using System.Reflection;
using Example.Api.Attributes;

namespace Example.Api.Managers;

public class RateLimitManager : IRateLimitManager
{

    private readonly ConcurrentDictionary<string, MethodInfo> _resources = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<DateTime>> _requests = new();
    private readonly ConcurrentDictionary<string, UserToken> _userAndOrigins = new();

    public void RegisterResource(string fullname, MethodInfo info)
    {
        if (_resources.ContainsKey(fullname)) 
            throw new InvalidOperationException("Resource already registered");
        
        _resources.TryAdd(fullname, info);
    }

    public bool CanPerformRequest(string fullName, UserToken token)
    {

        if (!_userAndOrigins.ContainsKey(token.UserName))
        {
            _userAndOrigins.TryAdd(token.UserName, token);
            _requests[token.UserName] = new ConcurrentBag<DateTime>();
        }

        var resource = _resources[fullName];

        var requests = _requests[token.UserName];

        var resultRateLimit = CheckIfRateLimitReached(requests.ToList(), resource);
        if (resultRateLimit)
            return false;

        var resultCoolingDown = CheckIfCoolingDownIsInProgress(requests.ToList(), resource);
        if (resultCoolingDown)
            return false;

        requests.Add(DateTime.Now); //new request made

        return true;
    }

    private bool CheckIfCoolingDownIsInProgress(List<DateTime> requests, MethodInfo resource)
    {
        //the logic for cooling down goes here
        return false;
    }

    private bool CheckIfRateLimitReached(List<DateTime> requests, MethodInfo resource)
    {
        var attribute = resource.GetCustomAttribute(typeof(RequestsPerTimespanAttribute)) as RequestsPerTimespanAttribute;
        if (attribute == null)
            return true; // this check passes since there is no attribute found to validate


        var milliseconds = attribute.Milliseconds * -1;
        var dateTime = DateTime.Now.AddMilliseconds(milliseconds);

        var cnt = 0;
        for (var i = requests.Count - 1; i >= 0; i--)
        {
             var request = requests[i];
             if (request > dateTime)
                 cnt++;
             else
                 break;
        }

        return cnt>= attribute.Rate;
    }
}


public record UserToken(string UserName, string Origin);