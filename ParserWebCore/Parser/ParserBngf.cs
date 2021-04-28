using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserBngf : ParserAbstract, IParser
    {
        private const int count = 30;

        public void Parsing()
        {
            Parse(ParsingBngf);
        }

        private void ParsingBngf()
        {
            for (var i = count; i >= 1; i--)
            {
                try
                {
                    ParsingPage($"https://bngf.ru/procurement/actual-procurement/?PAGEN_1={i}");
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
                    "//tr[starts-with(@id, 'bx_')]") ??
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
                .Trim().ReplaceHtmlEntyty();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://bngf.ru{href}";
            var purName = (n.SelectSingleNode(".//a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = href.ToMd5();
            var datePubT =
                (n.SelectSingleNode("./td[1]")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href, datePubT);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode("./td[2]")
                    ?.InnerText ?? "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var orgName = (n.SelectSingleNode("./td[4]")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var status = (n.SelectSingleNode("./td[5]")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var tn = new TenderBngf("АО «Башнефтегеофизика»",
                "https://bngf.ru/", 314,
                new TypeBnGf
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    OrgName = orgName,
                    Status = status,
                });
            ParserTender(tn);
        }
    }
}