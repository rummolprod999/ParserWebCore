using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserKzGroup : ParserAbstract, IParser
    {
        private const int Count = 5;

        public void Parsing()
        {
            Parse(ParsingKzGroup);
        }

        private void ParsingKzGroup()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"http://kzgroup.ru/rus/tenders?page={i}&text=";
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
            var s = DownloadString.DownL(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'tableBox')]/table/tr[position() > 1]") ??
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
            var href = (n.SelectSingleNode("./td[1]/a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"http://kzgroup.ru{href}";
            var purNum = href.GetDataFromRegex(@"/(\d+)/$");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var purName = (n.SelectSingleNode("./td[3]")
                               ?.InnerText ?? "").Trim();
            var orgName = (n.SelectSingleNode("./td[4]")
                               ?.InnerText ?? "").Trim();
            var datePubT =
                (n.SelectSingleNode("./td[5]")
                     ?.InnerText ?? "").Trim();
            datePubT = datePubT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode("./td[7]")
                     ?.InnerText ?? "").Trim();
            var dateEndT1 = dateEndT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var timeEndT = dateEndT.GetDataFromRegex(@"(\d{2}:\d{2})");
            dateEndT = $"{dateEndT1} {timeEndT}".Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                return;
            }

            var tn = new TenderKzGroup("ПАО «Кировский завод»", "http://kzgroup.ru", 67,
                new TypeKzGroup
                {
                    OrgName = orgName,
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