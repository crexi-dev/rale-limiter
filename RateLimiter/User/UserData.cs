namespace RateLimiter.User
{
    public class UserData : IUserData
    {
        public string? IpAddress { get; set; }
        public string? CountryCode { get; set; }
        public string? Token { get; set; }
    }
}
