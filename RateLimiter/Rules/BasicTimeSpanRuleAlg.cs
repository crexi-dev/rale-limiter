using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Rules
{
    /// <summary>
    /// a certain timespan has passed since the last call;
    /// </summary>
    public class BasicTimeSpanRule : IRuleAlg
    {
        public TimeSpan TimeSpan { get; set; }
        public List<string> LocationsFilter { get; set; }

        private DateTime lastRequestTime = DateTime.MinValue;

        public BasicTimeSpanRule(TimeSpan timeSpan, List<string> locations = null)
        {
            if (timeSpan.TotalSeconds == 0) throw new Exception("Timespan paramter must be valid.");
            this.TimeSpan = timeSpan;
            this.LocationsFilter = locations;
        }

        public bool SendRequest()
        {
            var now = DateTime.Now;

            if (lastRequestTime == DateTime.MinValue)
            {
                lastRequestTime = now;
                return true;
            }

            if (now.Subtract(lastRequestTime) > TimeSpan)
            {
                lastRequestTime = now;
                return true;
            }

            return false;
        }
    }
}
