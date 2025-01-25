using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter.Interfaces;

namespace RateLimiter.Identifiers
{
    sealed class Token : IIdentifier
    {
        private readonly string Token { get; set; }

        public Token(string token)
        {
            Token = token;
        }

        public string ToString()
        {
            return Token;
        }
    }
}
