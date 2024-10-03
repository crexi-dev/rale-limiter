public interface IRateLimitRule
{
  bool IsRequestAllowed(string clientId, string resource, string region, string token);
}
