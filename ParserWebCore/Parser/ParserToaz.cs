using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserToaz : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingToaz);
        }

        private void ParsingToaz()
        {
            try
            {
                for (int i = 1; i < 10; i++)
                {
                    ParsingPage($"https://www.toaz.ru/zakupki/zakupki-tovarov/?page={i}");
                }

                for (int i = 1; i < 10; i++)
                {
                    ParsingPage($"https://www.toaz.ru/zakupki/zakupki-uslug/?page={i}");
                }
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
                    "//article[contains(@class, 'news-article')]") ??
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
            var purName =
                n.SelectSingleNode(".//h2")?.InnerText?.Trim() ??
                throw new Exception(
                    $"cannot find purName in {href}");
            var status = (n.SelectSingleNode(".//span[contains(.,'Пароль для скачивания')]")?.InnerText ?? "")
                .Trim();
            var datePubT =
                (n.SelectSingleNode(".//span[contains(.,'Дата размещения')]")
                    ?.InnerText ?? "");
            datePubT = datePubT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEndT =
                (n.SelectSingleNode(".//span[contains(.,'Дата окончания приема')]")
                    ?.InnerText ?? "");
            dateEndT = dateEndT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4} \d{2}:\d{2})");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            var tn = new TenderToaz("ПАО «ТОАЗ»", "https://www.toaz.ru/", 389,
                new TypeToaz
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                    Status = status
                });
            ParserTender(tn);
        }
    }
}