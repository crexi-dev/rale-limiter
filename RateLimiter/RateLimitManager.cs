using System;
using System.Collections.Concurrent;
using Microsoft.VisualBasic;
using RateLimiter.Attributes;

namespace RateLimiter;

public class RateLimitManager : IRateLimitManager
{
    private readonly ConcurrentDictionary<string, ConcurrentStack<DateTime>> _requests = new();
    private readonly ConcurrentDictionary<string, UserToken> _userAndOrigins = new();

    public bool AddNewRequest(IRateLimit attribute, UserToken token)
    {

        if (!_userAndOrigins.ContainsKey(token.UserName))
        {
            _userAndOrigins.TryAdd(token.UserName, token);
            _requests[token.UserName] = new ConcurrentStack<DateTime>();
        }

        var requests = _requests[token.UserName];
        if (attribute is CooldownPeriodAttribute cooldown)
        {
            if (requests.IsEmpty)
            {
                requests.Push(DateTime.Now);
                return true;
            }

            var dateTime = DateTime.Now.AddMilliseconds(cooldown.TimeSpan.TotalMicroseconds * -1);
            requests.TryPop(out var request);
            return dateTime >= request;
        }




        return false;
    }
}


public record UserToken(string UserName, string Origin);