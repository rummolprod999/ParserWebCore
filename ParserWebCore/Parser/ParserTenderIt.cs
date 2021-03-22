using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserTenderIt : ParserAbstract, IParser
    {
        private const string Urlpage = "https://tenderit.ru/home/index?page=";

        public void Parsing()
        {
            Parse(ParsingTpta);
        }

        private void ParsingTpta()
        {
            for (var i = 1; i <= 10; i++)
            {
                try
                {
                    ParsingPage($"{Urlpage}{i}");
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
                    "//tr[@data-key]") ??
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

            href = $"https://tenderit.ru{href}";
            var purNum = href.GetDataFromRegex(@"id=(\d+)");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var purName = (n.SelectSingleNode(".//td[2]//a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var orgName = (n.SelectSingleNode(".//td[3]")
                ?.InnerText ?? "").ReplaceHtmlEntyty().Trim();
            var city = (n.SelectSingleNode(".//td[5]")
                ?.InnerText ?? "");
            var datePubT =
                (n.SelectSingleNode(".//td[1]")
                    ?.InnerText ?? "");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode(".//td[6]")
                    ?.InnerText ?? "").Replace("Дата окончания:", "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                return;
            }

            var tn = new TenderTenderIt("Тендерит", "https://tenderit.ru/", 305,
                new TypeTenderIt
                {
                    OrgName = orgName,
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    City = city
                });
            ParserTender(tn);
        }
    }
}