using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserDpd : ParserAbstract, IParser
    {
        private static string _startUrl = "https://www1.dpd.ru/tenders/";

        public void Parsing()
        {
            Parse(ParsingDpd);
        }

        private void ParsingDpd()
        {
            var s = DownloadString.DownLUserAgent(_startUrl);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", _startUrl);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//a[starts-with(@href, '/tenders/index.php?r=site')]") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserTendersList(a);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTendersList(HtmlNode n)
        {
            var href = (n?.Attributes["href"]?.Value ?? "")
                .Trim().ReplaceHtmlEntyty().Replace("&amp;", "&");
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://www1.dpd.ru{href}";
            var s = DownloadString.DownLUserAgent(href);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", href);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//div[@class = 'list-view']//div[@class = 'view']") ??
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
                .Trim().ReplaceHtmlEntyty().Replace("&amp;", "&");
            ;
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://www1.dpd.ru{href}";
            var purName = (n.SelectSingleNode(".//a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = href.GetDataFromRegex(@"id=(\d+)");
            var datePubT =
                (n.SelectSingleNode("./b[. = 'Дата открытия торгов:']/following-sibling::text()")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("yyyy-MM-dd");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href, datePubT);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode("./b[. = 'Дата закрытия торгов:']/following-sibling::text()")
                    ?.InnerText ?? "").Trim();
            var dateEnd = dateEndT.ParseDateUn("yyyy-MM-dd");
            var tn = new TenderDpd("DPDgroup",
                "https://www1.dpd.ru/", 311,
                new TypeDpd
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName
                });
            ParserTender(tn);
        }
    }
}