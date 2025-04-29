#region

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using AngleSharp.Parser.Html;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;

#endregion

namespace ParserWebCore.SharedLibraries
{
    public static class SharedTekTorg
    {
        public static int GetCountPage(string url)
        {
            var i = 1;
            var s = DownloadString.DownLTektorg(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger(
                    $"Empty string in {typeof(SharedTekTorg).Name}.{MethodBase.GetCurrentMethod().Name}",
                    url);
                return i;
            }

            var parser = new HtmlParser();
            var document = parser.Parse(s);
            var pages = document.QuerySelectorAll("ul.pagination:first-of-type > li > a[aria-label *= Страница]");
            if (pages.Length == 0)
            {
                pages = document.QuerySelectorAll("ul.pagination:first-of-type > li > a[aria-label *= Page]");
            }

            if (pages.Length > 0)
            {
                i = (from p in pages where p == pages.Last() select int.Parse(p.TextContent)).First();
            }

            return i;
        }

        public static decimal ParsePrice(string s)
        {
            s = WebUtility.HtmlDecode(s);
            s = Regex.Replace(s, @"\s+", "");
            s = s.GetDataFromRegex(@"(\d+[\.,]?\d+)");
            s = s.Replace('.', ',');
            var d = 0.0m;
            try
            {
                IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "," };
                d = decimal.Parse(s, formatter);
            }
            catch (Exception)
            {
                //ignore
            }

            return d;
        }
    }
}