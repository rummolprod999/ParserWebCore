using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserPptk : ParserAbstract, IParser
    {
        private readonly int _count = 75;

        public void Parsing()
        {
            Parse(ParsingPptk);
        }

        private void ParsingPptk()
        {
            for (var i = _count; i >= 1; i--)
            {
                try
                {
                    ParsingPage($"https://pptk-mos.ru/zakupki/?PAGEN_1={i}");
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
                    "//tr[@data-url]") ??
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

            href = $"https://pptk-mos.ru{href}";
            var purName = (n.SelectSingleNode("./td[2]/a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = (n.SelectSingleNode("./td[4]/a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var cusName = (n.SelectSingleNode("./td[3]/a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var status = (n.SelectSingleNode("./td[1]/a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var pwName = (n.SelectSingleNode("./td[5]/a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var nmck = (n.SelectSingleNode("./td[6]/a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty().ExtractPriceNew();
            var datePubT =
                (n.SelectSingleNode("./td[7]/a")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEndT =
                (n.SelectSingleNode("./td[8]/a")
                    ?.InnerText ?? "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyyHH:mm");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href, datePubT);
                return;
            }

            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href, dateEndT);
                return;
            }

            var tn = new TenderPptk("ООО «ГЭХ Закупки»",
                "https://pptk-mos.ru/", 310,
                new TypePptk
                {
                    Status = status,
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    PwName = pwName,
                    CusName = cusName,
                    Nmck = nmck
                });
            ParserTender(tn);
        }
    }
}