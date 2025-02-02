namespace Crexi.RateLimiter.Rule.Enum;

public enum EvaluationType
{
    TimespanSinceLastCall = 1,
    CallsDuringTimespan = 2,
    WeAreTeasing = 3,
    WeArePseudoConfusing = 4,
}