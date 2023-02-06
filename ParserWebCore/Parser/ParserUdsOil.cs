using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserUdsOil : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingUdsOil);
        }

        private void ParsingUdsOil()
        {
            try
            {
                ParsingPage($"https://udsoil.ru/tenders/");
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
                    "//div[contains(@class, 'table-row table-row__close')]") ??
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
            var href = (n.SelectSingleNode("./div[6]//a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://udsoil.ru{href}";
            var purName = (n.SelectSingleNode("./div[2]/div")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = (n.SelectSingleNode("./div[1]/div")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var delivPlace = (n.SelectSingleNode("./div[3]/div")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var datePub = DateTime.Now;
            var dateScoringT =
                (n.SelectSingleNode("./div[5]/div")
                    ?.InnerText ?? "").Trim();
            var dateScoring = dateScoringT.ParseDateUn("dd.MM.yyyy");
            var dateEndT =
                (n.SelectSingleNode("./div[4]/div")
                    ?.InnerText ?? "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            var tn = new TenderUdsOil("«UDS OIL» Удмуртия",
                "https://udsoil.ru/", 381,
                new TypeUdsOil
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    DateScoring = dateScoring,
                    DelivPlace = delivPlace
                });
            ParserTender(tn);
        }
    }
}