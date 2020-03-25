using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserRavis : ParserAbstract, IParser
    {
        private readonly string urlpage = "http://ravistender.ru";

        public void Parsing()
        {
            Parse(ParsingRavis);
        }

        private void ParsingRavis()
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
                    "//table[@class = 'tender-table']/tbody/tr") ??
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
            var href = urlpage;
            var purName =
                n.SelectSingleNode("./td[1]")?.InnerText?.Trim() ??
                throw new Exception(
                    $"Can not find purName in {href}");
            var purNum = purName.ToMd5();
            var dates = n.SelectSingleNode("./td[3]")?.InnerText?.Trim() ??
                        throw new Exception(
                            $"Can not find dates in {href}");
            var datePubT = dates.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4} \d{2}:\d{2})");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm");
            var dateEndT = dates.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4} \d{2}:\d{2})$");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            var tn = new TenderRavis("ООО \"Равис - птицефабрика Сосновская\"", "http://ravistender.ru", 256,
                new TypeRavis
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd
                });
            ParserTender(tn);
        }
    }
}