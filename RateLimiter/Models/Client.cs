namespace Crexi.API.Common.RateLimiter.Models
{
    public class Client
    {
        public string Id { get; }
        public string Location { get; }

        public Client(string id, string location)
        {
            Id = id;
            Location = location;
        }
    }

}
