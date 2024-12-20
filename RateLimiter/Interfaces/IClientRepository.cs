namespace RateLimiter.Interfaces
{
    public interface IClientRepository
    {
        /// <summary>
        /// Based on the client's access token, return their corresponding region.
        /// Note: This is a fake repository that returns example data.
        /// </summary>
        /// <param name="accessToken">Requesting client's access token</param>
        /// <returns>Region of the client</returns>
        string GetClientRegionByAccessToken(string accessToken);
    }
}
