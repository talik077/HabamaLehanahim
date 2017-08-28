using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Utils
{
    public static class StringExtensions
    {
        public static string Without(this string baseString, string substring)
        {
            return baseString.ReplaceAll(substring, "");
        }

        public static string ReplaceAll(
            this string baseString,
            string toReplace,
            string replaceWith)
        {
            while (baseString.Contains(toReplace))
            {
                baseString = baseString.Replace(toReplace, replaceWith);
            }
            return baseString;
        }
    }
}