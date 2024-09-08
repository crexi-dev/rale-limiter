using RateLimiter.Enums;

namespace RateLimiter.Repositories
{
    public static class ResourceRules
    {
        private static Dictionary<string, List<RulesEnum>> Rules { get; set; } = new();

        public static List<RulesEnum> GetRules(string resource)
        {
            if (Rules.TryGetValue(resource, out List<RulesEnum>? value))
            {
                return value;
            }

            throw new InvalidOperationException($"No rules set for resource {resource}");
        }

        public static void SaveRules(string resource, List<RulesEnum> rules)
        {
            Rules[resource] = rules;
        }
    }
}