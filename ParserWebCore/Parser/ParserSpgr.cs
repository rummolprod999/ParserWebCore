using System;
using System.Net;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSpgr : ParserAbstract, IParser
    {
        public static CookieCollection Cookies;
        public static readonly string HttpsT2Federal1Ru = "http://procurement.spgr.ru";
        private readonly GetCookieServiceSpgr _cookieService = GetCookieServiceSpgr.CreateInstance();

        public void Parsing()
        {
            Parse(ParsingSpgr);
        }

        private void ParsingSpgr()
        {
            try
            {
                GetPage();
            }
            catch (Exception e)
            {
                Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
            }
        }

        private void GetPage()
        {
            Cookies = _cookieService.CookieValue();
            for (var i = 1; i <= 10; i++)
            {
                GetPage($"https://procurement.spgr.ru/tender/?login=yes&PAGEN_1={i}");
            }
        }

        private void GetPage(string url)
        {
            var s = DownloadString.DownLHttpPostWithCookiesAll(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//tr[contains(@class, 'js-lot-row')]") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserTender(a, url);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTender(HtmlNode n, string _href)
        {
            var href = (n.SelectSingleNode(".//a[contains(@class, 't_lot_title')]")?.Attributes["href"]
                    ?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"http://procurement.spgr.ru/tender/{href}";
            var purName =
                n.SelectSingleNode(".//a[contains(@class, 't_lot_title')]")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find purName in {_href}");
            var purNum =
                n.SelectSingleNode(".//span[contains(@class, 't_lot_id')]")?.InnerText?.Replace("Лот №", "")
                    .Replace(":", "").Trim() ??
                throw new Exception(
                    $"Cannot find purNum in {_href}");
            purNum = purNum.GetDataFromRegex(@"(\d+)");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("bad purNum", purName);
                return;
            }

            var endDateT =
                n.SelectSingleNode(".//span[contains(@class, 't_lot_end_date')]/nobr")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find endDateT in {_href}");
            var dateEnd = endDateT.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            var pubDateT =
                n.SelectSingleNode(".//span[contains(@class, 't_lot_start_date')]/nobr")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find pubDateT in {_href}");
            var datePub = pubDateT.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            var tn = new TenderSpgr("Группа компаний «Спектрум»", "http://procurement.spgr.ru/", 337,
                new TypeSpgr
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd
                });
            ParserTender(tn);
        }
    }
}