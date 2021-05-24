using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserKopeMash : ParserAbstract, IParser
    {
        private const int Count = 5;

        public void Parsing()
        {
            Parse(ParsingKopemash);
        }

        private void ParsingKopemash()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"https://www.kopemash.ru/company/tenders/?PAGEN_1={i}";
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
                htmlDoc.DocumentNode.SelectNodes("//p[@class = 'news-item']") ??
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

            href = $"https://www.kopemash.ru{href}";
            var purNum = href.GetDataFromRegex(@"(\d+)\.html");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var purName = n.SelectSingleNode(".//a")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find purName in {href}");
            var datePubT =
                (n.SelectSingleNode(".//span[@class = 'news-date-time']")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEnd = datePub.AddDays(2);
            var tn = new TenderKopeMash("Акционерное общество «Копейский машиностроительный завод»",
                "https://www.kopemash.ru/", 320,
                new TypeKopeMash
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                });
            ParserTender(tn);
        }
    }
}