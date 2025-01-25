using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    sealed class FixedWindow : IRule
    {
        private readonly IFixedWindowHistory FixedWindowHistory;
        private readonly uint MaxCount;
        private readonly uint Window;

        public FixedWindow(IFixedWindowHistory fixedWindowHistory, uint maxCount, uint window)
        {
            FixedWindowHistory = fixedWindowHistory;
            MaxCount = maxCount;
            Window = window;
        }

        public bool Check(IIdentifier identifier)
        {
            var now = DateTime.Now();
            var history = FixedWindowHistory.GetRequestCount(identifier, now.AddSeconds(-Window), now);
            var isAllowed = history <= MaxCount;

            if (isAllowed)
            {
                FixedWindowHistory.Record(identifier, now);
            }

            return isAllowed;
        }
    }
}
