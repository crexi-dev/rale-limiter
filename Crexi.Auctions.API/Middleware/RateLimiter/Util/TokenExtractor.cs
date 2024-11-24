public class TokenExtractor : ITokenExtractor
{
    public string ExtractToken(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            var token = authorizationHeader.ToString().Split(' ').Last();
            return token;
        }

        return null;
    }
}
