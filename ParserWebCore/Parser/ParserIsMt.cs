using System;
using System.Web;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserIsMt : ParserAbstract, IParser
    {
        private const int Count = 5;

        public void Parsing()
        {
            Parse(ParsingIsmt);
        }

        private void ParsingIsmt()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"http://is-mt.pro/Purchase/ListPurchase?Page={i}&SearchType=Purchase";
                try
                {
                    ParsingPage(urlpage);
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
                htmlDoc.DocumentNode.SelectNodes(
                    "//table[@class = 'list-table']//tr[@class = 'item']") ??
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
            var href = (n.SelectSingleNode(".//td[3]/a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"http://is-mt.pro/{href}";
            var purName = n.SelectSingleNode(".//td[3]/a")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find purName in {href}");
            purName = HttpUtility.HtmlDecode(purName);
            var purNum = href.GetDataFromRegex(@"id=(.+)$");
            var datePubT = n.SelectSingleNode(".//td[1]")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find datePubT in {href}");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEndT = n.SelectSingleNode(".//td[2]")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find dateEndT in {href}");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            var status = n.SelectSingleNode(".//td[4]")?.InnerText?.Trim();
            var tn = new TenderIsMt("«Маркетинговые технологии»", "http://is-mt.pro/", 278,
                new TypeIsMt
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                    Status = status
                });
            ParserTender(tn);
        }
    }
}