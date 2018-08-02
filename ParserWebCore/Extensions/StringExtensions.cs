using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ParserWebCore.Logger;

namespace ParserWebCore.Extensions
{
    public static class StringExtensions
    {
        public static string Win1251ToUtf8(this string s)
        {
            var windows1251 = Encoding.GetEncoding("windows-1251");
            var utf8 = Encoding.UTF8;
            var originalBytes = windows1251.GetBytes(s);
            return utf8.GetString(originalBytes);
        }

        public static DateTime ParseDateUn(this string s, string form)
        {
            var d = DateTime.MinValue;
            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    d = DateTime.ParseExact(s, form, CultureInfo.InvariantCulture);
                }
                catch
                {
                    // ignored
                }
            }

            return d;
        }

        public static string GetDateFromRegex(this string s, string r)
        {
            var ret = "";
            try
            {
                var regex = new Regex(r, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = regex.Matches(s);
                if (matches.Count > 0)
                {
                    ret = matches[0].Groups[1].Value.Trim();
                }
            }
            catch (Exception e)
            {
                Log.Logger(e, r);
            }

            return ret;
        }

        public static string GetDateWithMonth(this string s)
        {
            if (s.Contains("янв")) return s.Replace("янв", "01");
            if (s.Contains("фев")) return s.Replace("фев", "02");
            if (s.Contains("мар")) return s.Replace("мар", "03");
            if (s.Contains("апр")) return s.Replace("апр", "04");
            if (s.Contains("ма")) return s.Replace("ма", "05");
            if (s.Contains("июн")) return s.Replace("июн", "06");
            if (s.Contains("июл")) return s.Replace("июл", "07");
            if (s.Contains("авг")) return s.Replace("авг", "08");
            if (s.Contains("сен")) return s.Replace("сен", "09");
            if (s.Contains("окт")) return s.Replace("окт", "10");
            if (s.Contains("ноя")) return s.Replace("ноя", "11");
            return s.Contains("дек") ? s.Replace("дек", "12") : "";
        }
    }
}