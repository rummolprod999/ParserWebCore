using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSibintek : ParserAbstract, IParser
    {
        private const int Count = 5;

        public void Parsing()
        {
            Parse(ParsingSibintek);
        }

        private void ParsingSibintek()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"http://sibintek.ru/company/zakupki/?PAGEN_1={i}";
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
                htmlDoc.DocumentNode.SelectNodes("//table[@class = 'zakupki']/tr[@class = 'news-item']") ??
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

            href = $"http://sibintek.ru{href}";
            var purNum = (n.SelectSingleNode("./td[2]")?.InnerText ?? "")
                .Trim();
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var purName = n.SelectSingleNode(".//a")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find purName in {href}");
            var datePubT =
                (n.SelectSingleNode("./td[1]/span")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode("./td[4]/div")
                    ?.InnerText ?? "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                return;
            }

            var status = (n.SelectSingleNode("./td[5]")
                ?.InnerText ?? "").Trim();
            var tn = new TenderSibintek("ООО ИК «СИБИНТЕК»", "http://sibintek.ru", 71,
                new TypeSibIntek
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Status = status
                });
            ParserTender(tn);
        }
    }
}