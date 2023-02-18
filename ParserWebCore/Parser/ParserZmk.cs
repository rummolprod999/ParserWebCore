using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserZmk : ParserAbstract, IParser
    {
        private const int Count = 15;

        public void Parsing()
        {
            Parse(ParsingZmk);
        }

        private void ParsingZmk()
        {
            ParsingPage("https://www.zmk.ru/category/tender/");
            for (var i = 2; i <= Count; i++)
            {
                var urlpage = $"https://www.zmk.ru/category/tender/page/{i}/";
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
            var s = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes("//article[@id]") ??
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

            var purName = n.SelectSingleNode(".//h2/a")?.InnerText?.Trim()?.ReplaceHtmlEntyty() ?? throw new Exception(
                $"cannot find purName in {href}");
            var purNum = purName.ToMd5();
            var datePubT =
                (n.SelectSingleNode(".//time[contains(@class, 'entry-date published')]")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode("./div[contains(., 'завершение')]")
                    ?.InnerText ?? "").Trim();
            var dateEndR = dateEndT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var dateEnd = dateEndR.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var tn = new TenderZmk("ООО «Златоустовский металлургический завод»", "https://www.zmk.ru/", 383,
                new TypeZmk
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