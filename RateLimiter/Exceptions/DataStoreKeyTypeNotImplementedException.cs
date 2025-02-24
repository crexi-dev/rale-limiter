using System;

namespace RateLimiter.Exceptions
{
    public class DataStoreKeyTypeNotImplementedException : Exception
    {
        private const string _message = "The requested data store key type has not been implemented";

        public DataStoreKeyTypeNotImplementedException() : base(_message) { }
    }
}
