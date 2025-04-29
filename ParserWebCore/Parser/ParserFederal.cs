#region

using System;
using System.Net;
using System.Reflection;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserFederal : ParserAbstract, IParser
    {
        public static Cookie Cookie;
        public static readonly string HttpsT2Federal1Ru = "https://t2.federal1.ru/";
        private readonly CookieService _cookieService = GetCookieServiceFederal.CreateInstance();

        public void Parsing()
        {
            Parse(ParsingFederal);
        }

        private void ParsingFederal()
        {
            try
            {
                GetPage();
            }
            catch (Exception e)
            {
                Log.Logger($"Error in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}", e);
            }
        }

        private void GetPage()
        {
            Cookie = _cookieService.CookieValue();
            for (var i = 1; i < 6; i++)
            {
                GetPage($"https://t2.federal1.ru/registry/list/?page={i}");
            }
        }

        private void GetPage(string url)
        {
            var s = DownloadString.DownLHttpPostWithCookies(url, HttpsT2Federal1Ru, Cookie);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//table[contains(@class, 'mte-grid-table')]/tbody/tr") ??
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
                n.SelectSingleNode("./td[position() = 3]/a")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find purName in {_href}");
            var purNum =
                n.SelectSingleNode("./td[position() = 1]")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find purNum in {_href}");
            var cusName =
                n.SelectSingleNode("./td[position() = 2]")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find cusName in {_href}");
            var nmckT =
                n.SelectSingleNode("./td[position() = 4]")?.InnerText?.Trim() ?? "";
            var (nmck, currency) = nmckT.GetTwoDataFromRegex("([\\d\\s.]+)(.*)");
            nmck = nmck.DelAllWhitespace();
            var href =
                n.SelectSingleNode("./td[position() = 3]/a")?.Attributes["href"]?.Value ??
                throw new Exception(
                    $"Cannot find href in {_href}");
            var status =
                n.SelectSingleNode("./td[position() = 7]")?.InnerText?.Trim() ?? "";
            var pwName =
                n.SelectSingleNode("./td[position() = 6]")?.InnerText?.Trim() ?? "";
            var endDateT =
                n.SelectSingleNode("./td[position() = 5]")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Cannot find endDateT in {_href}");
            var dateEndT = endDateT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4}\s+\d{2}:\d{2})");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            var tn = new TenderFederal("ЭТП \"Федерация\"", "https://t2.federal1.ru/", 294,
                new TypeFederal
                {
                    PurName = purName, PurNum = purNum, DatePub = DateTime.Now, Href = href, DateEnd = dateEnd,
                    PwName = pwName, Currency = currency, Status = status, Nmck = nmck, CusName = cusName
                });
            ParserTender(tn);
        }
    }
}