﻿using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
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

        public static string GetDataFromRegex(this string s, string r)
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

        public static (string, string) GetTwoDataFromRegex(this string s, string r)
        {
            try
            {
                var regex = new Regex(r, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = regex.Matches(s);
                if (matches.Count > 0)
                {
                    return (matches[0].Groups[1].Value.Trim(), matches[0].Groups[2].Value.Trim());
                }
            }
            catch (Exception e)
            {
                Log.Logger(e, r);
            }

            return ("", "");
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

        public static string DelDoubleWhitespace(this string s)
        {
            var resString = Regex.Replace(s, @"\s+", " ");
            resString = resString.Trim();
            return resString;
        }

        public static string DelAllWhitespace(this string s)
        {
            var resString = Regex.Replace(s, @"\s+", "");
            resString = resString.Trim();
            return resString;
        }

        public static string ToMd5(this string s)
        {
            using (var md = MD5.Create())
            {
                var data = md.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sBuilder = new StringBuilder();
                foreach (var v in data)
                {
                    sBuilder.Append(v.ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public static string ReplaceHtmlEntyty(this string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

        public static string ExtractPrice(this string s)
        {
            var price = "";
            price = HttpUtility.HtmlDecode(s);
            price = price.GetDataFromRegex(@"([\d.,\s]+)").DelAllWhitespace().Replace(",", "").Trim();
            return price;
        }
    }
}