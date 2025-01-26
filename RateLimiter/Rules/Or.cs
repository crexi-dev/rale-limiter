using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    public class Or : IRule
    {
        private readonly IRule[] Rules;

        public Or(IRule[] rules)
        {
            Rules = rules;
        }

        public bool Check(IIdentifier identifier)
        {
            foreach (var rule in Rules)
            {
                if(rule.Check(identifier))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
