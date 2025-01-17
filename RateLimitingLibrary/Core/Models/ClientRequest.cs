using System;

namespace RateLimitingLibrary.Core.Models
{
    /// <summary>
    /// Represents a client request with metadata.
    /// </summary>
    public class ClientRequest
    {
        public string ClientToken { get; set; }
        public string Resource { get; set; }
        public DateTime RequestTime { get; set; }
    }
}