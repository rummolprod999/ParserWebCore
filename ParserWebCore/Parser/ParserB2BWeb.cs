using System;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using ParserWebCore.BuilderApp;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserB2BWeb : ParserAbstract, IParser
    {
        private const int MaxPage = 5;
        public static CookieCollection CookieCollection;
        private readonly CookiesB2B _cookieService = CookiesB2B.CreateInstance();

        public void Parsing()
        {
            Parse(ParsingB2B);
        }

        private void ParsingB2B()
        {
            for (var i = 0; i <= MaxPage; i++)
            {
                try
                {
                    CookieCollection = _cookieService.CookieValue();
                    GetPage($"https://www.b2b-center.ru/market/?from={i * 20}");
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
                }
            }
        }

        private void GetPage(string url)
        {
            var result = DownloadString.DownLHttpPostWithCookiesB2b(url, CookieCollection, useProxy: Builder.UserProxy);
            if (string.IsNullOrEmpty(result))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    url);
                return;
            }

            if (result.Contains("не допускает использование ботов"))
            {
                Log.Logger("Google Captcha");
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(result);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//table[contains(@class, 'search-results')]/tbody/tr") ??
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
            var purName =
                n.SelectSingleNode("./td[1]/a/div")?.InnerText?.Trim().ReplaceHtmlEntyty() ??
                throw new Exception(
                    $"Cannot find purName in {_href}");
            var fullPw =
                n.SelectSingleNode("./td[1]/a")?.InnerText?.Trim().ReplaceHtmlEntyty() ??
                throw new Exception(
                    $"Cannot find fullPw in {_href}");
            var href = (n.SelectSingleNode("./td[1]/a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href", purName);
                return;
            }

            href = $"https://www.b2b-center.ru{href}";
            var pwName = fullPw.GetDataFromRegex(@"(.+) №\s+\d+");
            var purNum = fullPw.GetDataFromRegex(@"№\s+(\d+)");
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var orgName =
                n.SelectSingleNode("./td[2]/a")?.InnerText?.Trim().ReplaceHtmlEntyty() ??
                throw new Exception(
                    $"Cannot find orgName in {href}");
            var pubDateT =
                n.SelectSingleNode("./td[3]")?.InnerText?.Trim().ReplaceHtmlEntyty() ??
                throw new Exception(
                    $"Cannot find pubDateT in {href}");
            var datePub = pubDateT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (datePub == DateTime.MinValue)
            {
                datePub = DateTime.Today;
            }

            var endDateT =
                n.SelectSingleNode("./td[4]")?.InnerText?.Trim().ReplaceHtmlEntyty() ??
                throw new Exception(
                    $"Cannot find endDateT in {href}");
            var dateEnd = endDateT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = DateTime.Today.AddDays(2);
            }

            var tn = new TenderB2BWeb("Электронная торговая площадка B2B-Center", "https://www.b2b-center.ru/", 299,
                new TypeB2B
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                    PwName = pwName, FullPw = fullPw, OrgName = orgName
                });
            Thread.Sleep(10000);
            ParserTender(tn);
        }
    }
}