using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;

namespace RateLimiter.Identifiers
{
    public class Token : IIdentifier
    {
        private  string TokenKey { get; set; }

        public Token(string token)
        {
            TokenKey = token;
        }

        public string ToString()
        {
            return TokenKey;
        }
    }
}
