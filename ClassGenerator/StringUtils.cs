using System;
using System.Collections.Generic;
using System.Text;

namespace ClassGenerator
{
    public static class StringUtils
    {
        public static string UppercaseFirst(string s)
        {
            if (s == null)
            {
                return null;
            }
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
