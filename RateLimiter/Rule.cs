using System;
using System.Runtime.InteropServices.ComTypes;

namespace RateLimiter
{
    public class Rule
    {
        private const StringComparison ScopeComparison = StringComparison.InvariantCultureIgnoreCase;

        public string Scope { get; }
        public int Limit { get; }
        public TimeSpan Window { get; }

        public Predicate<RateLimitRequest> AppliesWhen { get; set; } = _ => true;

        public Rule(string scope, int limit, TimeSpan window)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException($"Argument {scope} must not be null or whitespace.");
            }

            if (limit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), $"Argument {nameof(limit)} must be greater than zero. The provided value was {limit}.");
            }

            if (window <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(window), $"Argument {nameof(window)} must be a timespan value greater than zero. The provided value was {window}.");
            }

            Scope = scope;
            Limit = limit;
            Window = window;
        }

        public override bool Equals(object? other)
        {
            if (other == null)
            {
                return false;
            }

            return other is Rule rule && this.Equals(rule);


        }
        public bool Equals(Rule other) =>
            string.Equals(Scope, other.Scope, ScopeComparison);

        public override int GetHashCode()
        {
            return Scope.GetHashCode(ScopeComparison);
        }
    }
}
