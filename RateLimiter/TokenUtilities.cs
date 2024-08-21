using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter
{
    public static class TokenUtilities
    {
        /// <summary>
        /// Extract Mock Location info from mock accessToken.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static string ExtractLocation(string accessToken)
        {
            if (accessToken == null) return string.Empty;

            return accessToken.Substring(0, 2);
        }
    }
}
