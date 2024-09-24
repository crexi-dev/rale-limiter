using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Extensions
{
    /// <summary>
    /// Extension class for string.
    /// </summary>
    public static class StringExtensions
    {
        public static bool CaseInsensitiveEquals(this string a, string b)
        {
            return a?.Equals(b, StringComparison.CurrentCultureIgnoreCase) ?? false;
        }
    }
}
