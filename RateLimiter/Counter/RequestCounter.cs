using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using RateLimiter.Stores;

namespace RateLimiter.Counter
{
    public class RequestCounter : IRequestCounter
    {
        private readonly ChannelWriter<NewRequest> writer;

        public RequestCounter(ChannelWriter<NewRequest> writer)
        {
            this.writer = writer;
        }

        public async Task RecordRequest(string id, DateTimeOffset utcTime)
        {
            await this.writer.WriteAsync(new NewRequest(id, utcTime));
        }
    }
}
