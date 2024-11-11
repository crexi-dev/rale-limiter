namespace RateLimiter.Repository.TrafficRepository;

public interface ITrafficRepository
{
    /// <summary>
    /// Record new traffic.
    /// </summary>
    /// <param name="traffic">The traffic to record. </param>
    /// <returns></returns>
    public Task RecordTraffic(Traffic traffic);

    /// <summary>
    /// Get the last recorded traffic entry.
    /// </summary>
    /// <param name="for_token"> the source (token) of the traffic. </param>
    /// <param name="for_resource"> the identifier for the resource being requested </param>
    /// <returns></returns>
    public Task<Traffic?> GetLatestTraffic(string for_token, string for_resource);

    /// <summary>
    /// Get all the traffic recorded within the specific time span, optionally limited to a specific timespan.
    /// </summary>
    /// <param name="for_token"> the source (token) of the traffic </param>
    /// <param name="for_resource"> the identifier for the resource being requested </param>
    /// <param name="within_span"> optional filter operation to traffic within the specific time span. </param>
    /// <returns></returns>
    public Task<IEnumerable<Traffic>> GetTraffic(string for_token, string for_resource, TimeSpan? within_span = null);

    /// <summary>
    /// Get the Total number of Requests from the source (token).
    /// </summary>
    /// <param name="for_token"> the source (token) of the traffic, optionally limited to a specific timespan. </param>
    /// <param name="for_resource"> the identifier for the resource being requested </param>
    /// <param name="within_span"> optional filter operation to traffic within the specific time span. </param>
    /// <returns></returns>
    public Task<int> CountTraffic(string for_token, string for_resource, TimeSpan? within_span = default);

    /// <summary>
    /// Expire / Delete all traffic from the source (token), optionally limited to a specific timespan. 
    /// </summary>
    /// <param name="for_token"> the source (token) of the traffic. </param>
    /// <param name="for_resource"> the identifier for the resource being requested </param>
    /// <param name="within_span"> optional filter operation to traffic within the specific time span. </param>
    /// <returns> number of traffic entries affected/expired </returns>
    public Task<int> ExpireTraffic(string for_token, string for_resource, TimeSpan? within_span = default);
}
