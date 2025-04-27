using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserMaxi : ParserAbstract, IParser
    {
        private string _startPage = "https://maxi-tender.ru/procurements";

        public void Parsing()
        {
            Parse(ParsingMaxi);
        }

        private void ParsingMaxi()
        {
            try
            {
                ParsingPage(_startPage);
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
                    "//a[contains(@class, 'tender-card')]") ??
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
            var href = (n.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://maxi-tender.ru{href}";
            var purNum = href.GetDataFromRegex(@"-(\d+)$");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var purName = n.SelectSingleNode(".//div[@class = 'tender-card__title']//span")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find purName in {href}");
            var dates = n.SelectSingleNode(".//div[@class = 'tender-card__date__item']//span")?.InnerText?.DelDoubleWhitespace().Trim() ?? throw new Exception(
                $"cannot find dates in {href}");
            var datePubT = dates.GetDataFromRegex(@"^\s*(\d{2}\.\d{2}\.\d{4})");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT = dates.GetDataFromRegex(@"\s*(\d{2}\.\d{2}\.\d{4})$");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                return;
            }

            var tn = new TenderMaxi("Макси Девелопмент", "http://maxi-cre.ru", 83, new TypeMaxi
            {
                DateEnd = dateEnd,
                DatePub = datePub,
                Href = href,
                PurName = purName,
                PurNum = purNum
            });
            ParserTender(tn);
        }
    }
}