namespace RateLimiter.Discriminators
{
    public class DiscriminatorEvaluationResult(string discriminatorName)
    {
        public string DiscriminatorName { get; init; } = discriminatorName;

        public bool IsMatch { get; set; }

        public string MatchValue { get; set; }

        public string? AlgorithmName { get; set; }
    }
}
