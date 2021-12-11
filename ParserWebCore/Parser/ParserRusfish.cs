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
                ParsingPage("https://russianfishery.ru/suppliers/tenders/");
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
                htmlDoc.DocumentNode.SelectNodes("//div[@class = 'tender-list__item']") ??
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
            var purName = (n.SelectSingleNode(".//div[@class = 'tender-prev__name']")
                ?.InnerText ?? "").Trim();
            if (string.IsNullOrEmpty(purName))
            {
                Log.Logger("Empty purName");
                return;
            }

            var purNum = purName.ToMd5();

            var href =
                n.SelectSingleNode(".//a")?.Attributes["href"]?.Value ??
                throw new Exception(
                    $"Cannot find href in {purNum}");
            href = $"https://russianfishery.ru{href}";
            var status = "";
            var datePubT =
                (n.SelectSingleNode(".//div[@class = 'tender-prev__date']")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                datePub = DateTime.Today;
            }

            var dateEndT =
                (n.SelectSingleNode(".//div[@class = 'tender-prev__time']")
                    ?.InnerText ?? "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

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