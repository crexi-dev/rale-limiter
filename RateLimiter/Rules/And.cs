using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;

namespace RateLimiter.Rules
{
    public class And : IRule
    {
        private readonly IRule[] Rules;

        public And(IRule[] rules)
        {
            Rules = rules;
        }

        public bool Check(IIdentifier identifier)
        {
            foreach (var rule in Rules)
            {
                if(!rule.Check(identifier))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
