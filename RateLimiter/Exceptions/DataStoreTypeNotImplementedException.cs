using System;

namespace RateLimiter.Exceptions
{
    public class DataStoreTypeNotImplementedException : Exception
    {
        private const string _message = "The requested data store type has not been implemented";

        public DataStoreTypeNotImplementedException() : base(_message) { }
    }
}
