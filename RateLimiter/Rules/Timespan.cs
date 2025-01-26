using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    public class Timespan : IRule
    {
        private readonly ITimespanHistory TimespanHistory;
        private readonly uint TimespanValue;

        public Timespan(ITimespanHistory timespanHistory, uint timespan)
        {
            TimespanHistory = timespanHistory;
            TimespanValue = timespan;
        }

        public bool Check(IIdentifier identifier)
        {
            var now = DateTime.Now;
            var history = TimespanHistory.GetLastRequestDate(identifier);
            var isAllowed = history.AddSeconds(TimespanValue) <= now;

            if (isAllowed)
            {
                TimespanHistory.Record(identifier, now);
            }

            return isAllowed;
        }

    }
}
