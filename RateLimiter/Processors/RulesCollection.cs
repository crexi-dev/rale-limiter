using System.Collections.Generic;
using System.Linq;

namespace RateLimiter.Processors;

/// Represents a collection of rules.
/// The `RulesCollection` class is a generic class that holds a collection of rules. It provides methods to add rules to the collection.
/// @typeparam T The type of the rules in the collection.
/// /
public class RulesCollection<T>
{
    /// Represents a collection of rules.
    /// /
    public IEnumerable<T>? Rules { get; set; }

    /// <summary>
    /// Adds a rule to the RulesCollection.
    /// </summary>
    /// <typeparam name="T">The type of the rule.</typeparam>
    /// <param name="rule">The rule to be added to the RulesCollection.</param>
    public virtual void AddRule(T rule)
    {
        var list = Rules?.ToList() ?? [];
        list.Add(rule);
        Rules = list;
    }
}