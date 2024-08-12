namespace RateLimiter.Model
{
    public class Request : RequestDTO
    {
        public List<DateTime> AccessTime { get; set; } = new List<DateTime>();
    }
}