using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserRwBy : ParserAbstract, IParser
    {
        private const string StartUrl = "https://www.rw.by/corporate/tenders_and_procurement/";

        public void Parsing()
        {
            Parse(ParsingRwBy);
        }

        private void ParsingRwBy()
        {
            try
            {
                ParsingPage(StartUrl);
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
                    "//ul[@class = 'tender-items']/li") ??
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

            var purNum = href.ToMd5();
            var purName = n.SelectSingleNode(".//a")?.InnerText?.Trim() ?? throw new Exception(
                              $"Can not find purName in {href}");
            var datePubT = n.SelectSingleNode("./span")?.InnerText?.Trim() ?? throw new Exception(
                               $"Can not find datePubT in {href}");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var tn = new TenderRwBy("Белорусская железная дорога", "https://www.rw.by", 128,
                new TypeRwBy {PurName = purName, PurNum = purNum, DatePub = datePub, Href = href});
            ParserTender(tn);
        }
    }
}