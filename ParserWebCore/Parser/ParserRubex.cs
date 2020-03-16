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
    public class ParserRubex : ParserAbstract, IParser
    {
        private readonly string urlpage = "https://rubexgroup.ru/%D0%B7%D0%B0%D0%BA%D1%83%D0%BF%D0%BA%D0%B8/";

        public void Parsing()
        {
            Parse(ParsingRubex);
        }

        private void ParsingRubex()
        {
            try
            {
                ParsingPage(urlpage);
            }
            catch (Exception e)
            {
                Log.Logger(e);
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
                    "//div[@class = 'tendorElem']") ??
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
            var href = (n.SelectSingleNode(".//a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            var purName = n.SelectSingleNode("./span")?.InnerText?.Trim() ?? throw new Exception(
                $"Can not find purName in {href}");
            purName = HttpUtility.HtmlDecode(purName);
            var fullDateNum = n.SelectSingleNode("./h2")?.InnerText?.Trim() ?? throw new Exception(
                $"Can not find fullDateNum in {href}");
            var purNum = fullDateNum.GetDataFromRegex(@"Извещение\s+№\s+(\d+)");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var datePubT = fullDateNum.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            if (string.IsNullOrEmpty(datePubT))
            {
                Log.Logger("Empty datePubT");
                return;
            }

            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEnd = DateTime.Now;
            var tn = new TenderRubex("RubEx Group", "https://rubexgroup.ru/", 248,
                new TypeRubex {PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd});
            ParserTender(tn);
        }
    }
}