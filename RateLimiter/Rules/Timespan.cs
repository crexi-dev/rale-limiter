using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    class Timespan : IRule
    {
        private readonly ITimespanHistory TimespanHistory;
        private readonly uint Timespan;

        public Timespan(ITimespanHistory timespanHistory, uint timespan)
        {
            TimespanHistory = timespanHistory;
            Timespan = timespan;
        }

        public bool Check(IIdentifier identifier)
        {
            var now = DateTime.Now();
            var history = TimespanHistory.GetLastRequestDate(identifier);
            var isAllowed = history.AddSecond(Timespan) <= now;

            if (isAllowed)
            {
                TimespanHistory.Record(identifier, now);
            }

            return isAllowed;
        }

    }
}
