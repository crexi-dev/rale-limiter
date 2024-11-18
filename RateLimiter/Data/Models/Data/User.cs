namespace RateLimiter.Data.Models
{
    public class User : BaseModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string TokenSource { get; set; }
    }
}
