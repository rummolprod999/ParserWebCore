using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserZdShip : ParserAbstract, IParser
    {
        private readonly List<string> Urlpages = new List<string>
        {
            "https://zdship.ru/tender"
        };

        public void Parsing()
        {
            Parse(ParsingTpta);
        }

        private void ParsingTpta()
        {
            foreach (var urlpage in Urlpages)
            {
                try
                {
                    for (int i = 1; i < 5; i++)
                    {
                        ParsingPage($"https://zdship.ru/tender?page={i}");
                    }
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
                    "//table[contains(@class, 'table')]/tbody/tr") ??
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
            var href = (n.SelectSingleNode(".//td[2]//a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"{href}";
            var purName = (n.SelectSingleNode(".//td[2]//a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = (n.SelectSingleNode(".//td[1]")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var datePubT =
                (n.SelectSingleNode(".//td[4]")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEndT =
                (n.SelectSingleNode(".//td[5]")
                    ?.InnerText ?? "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                return;
            }

            var status = (n.SelectSingleNode(".//td[6]")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var tn = new TenderZdship("Тендерная площадка ОАО «Зеленодольский завод имени A.M. Горького»",
                "http://tender.zdship.ru/", 307,
                new TypeZdShip
                {
                    Status = status,
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