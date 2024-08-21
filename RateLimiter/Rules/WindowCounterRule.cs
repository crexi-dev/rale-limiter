using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace RateLimiter.Rules
{
    /// <summary>
    /// X requests per timespan;
    /// </summary>
    public class WindowCounterRule : IRuleAlg
    {
        public int IntervalSeconds { get; set; }
        public int MaxRequests { get; set; }
        public List<string> LocationsFilter { get; set; }
        
        private int count;

        private Timer timer;

        public WindowCounterRule(int intervalSeconds, int maxRequests, List<string> locations = null)
        {
            this.IntervalSeconds = intervalSeconds;
            this.MaxRequests = maxRequests;
            this.LocationsFilter = locations;

            this.timer = new Timer(intervalSeconds * 1000);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            this.count = 0;
        }

        public bool SendRequest()
        {
            if (count < MaxRequests)
            {
                count++;
                return true;
            }

            return false;
        }
    }
}
