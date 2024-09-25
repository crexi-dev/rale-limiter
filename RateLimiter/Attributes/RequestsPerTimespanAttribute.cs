using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace RateLimiter.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequestsPerTimespanAttribute : Attribute, IRateLimit
{
    public int Rate { get; }
    public int Seconds { get; }

    public RequestsPerTimespanAttribute(int rate, int seconds)
    {
        Rate = rate;
        Seconds = seconds;
    }
}


public interface IRateLimit
{

}