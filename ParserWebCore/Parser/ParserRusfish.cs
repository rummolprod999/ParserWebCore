using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserRusfish : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingRusfish);
        }

        private void ParsingRusfish()
        {
            try
            {
                ParsingPage("https://russianfishery.ru/tenders/");
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
                htmlDoc.DocumentNode.SelectNodes("//div[@class = 'tenderlist_item']") ??
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
            var purName = (n.SelectSingleNode("./span")
                ?.InnerText ?? "").Trim();
            if (string.IsNullOrEmpty(purName))
            {
                Log.Logger("Empty purName");
                return;
            }

            var purNum = (n.Attributes["rel"]?.Value ?? "").Trim();
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var href = $"https://russianfishery.ru/tenders/item.php?page={purNum}";
            var status = (n.SelectSingleNode("./div[@class = 'tender_active']")
                ?.InnerText ?? "").Trim();
            var datePubT =
                (n.SelectSingleNode(".//div[@class = 'date_tag']")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                datePub = DateTime.Today;
            }

            var dateEnd = datePub.AddDays(2);
            var tn = new TenderRusfish("«Русская Рыбопромышленная Компания»", "https://russianfishery.ru/", 322,
                new TypeRusfish
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