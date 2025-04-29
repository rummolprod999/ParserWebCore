#region

using System;
using System.Web;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserAvtodis : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingAvtodis);
        }

        private void ParsingAvtodis()
        {
            for (var i = 1; i <= 4; i++)
            {
                try
                {
                    ParsingPage($"https://www.avtodispetcher.ru/consignor/page-{i}");
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(string url)
        {
            var s = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes("//table[@class = 'advs_list']//tr[contains(@class, 'data_row')]") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserTender(a);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTender(HtmlNode n)
        {
            var purName1 = (n.SelectSingleNode(".//td[1]")
                ?.InnerText ?? "").Trim().DelDoubleWhitespace();
            var purName2 = (n.SelectSingleNode(".//td[2]")
                ?.InnerText ?? "").Trim().DelDoubleWhitespace();
            var purName3 = (n.SelectSingleNode(".//td[3]")
                ?.InnerText ?? "").Trim().DelDoubleWhitespace();
            var purName = HttpUtility.HtmlDecode($"Откуда: {purName1} Куда: {purName2} Груз: {purName3}");
            var href =
                n.SelectSingleNode(".//a")?.Attributes["href"]?.Value ??
                throw new Exception(
                    $"Cannot find href in {purName}");
            href = $"https://www.avtodispetcher.ru{href}";
            var purNum = href.GetDataFromRegex("consignor/(\\d+).html");
            var tn = new TenderAvtodis("Автодиспетчер.Ру", "https://www.avtodispetcher.ru/", 402,
                new TypeAvtodis
                {
                    DateEnd = DateTime.Now.AddDays(2),
                    DatePub = DateTime.Now,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName
                });
            ParserTender(tn);
        }
    }
}