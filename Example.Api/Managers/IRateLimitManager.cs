
using Example.Api.Attributes;
using System.Reflection;

namespace Example.Api.Managers;

public interface IRateLimitManager
{
    public void RegisterResource(string fullname, MethodInfo info);
    bool CanPerformRequest(string fullName, UserToken token);
}