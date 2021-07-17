using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserLenReg : ParserAbstract, IParser
    {
        private readonly string _urlpage =
            "https://zakupki.lenreg.ru/Search/Requests?Urgent=False&Field=Weigh&Direction=Desc&Take=40&Page=";

        public int UPPER => 200;

        public void Parsing()
        {
            Parse(ParsingLenReg);
        }

        private void ParsingLenReg()
        {
            for (var i = UPPER; i >= 1; i--)
            {
                try
                {
                    ParsingPage(
                        $"https://zakupki.lenreg.ru/Search/Requests?Urgent=False&Field=LastChange&Direction=Desc&Take=40&Page={i}&DateFrom={DateTime.Today.AddDays(-30 * 6):MM}%2F{DateTime.Today.AddDays(-30 * 6):dd}%2F{DateTime.Today.AddDays(-30 * 6):yyyy}%2000%3A00%3A00&DateTo={DateTime.Today:MM}%2F{DateTime.Today:dd}%2F{DateTime.Today:yyyy}%2000%3A00%3A00");
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(string url)
        {
            var s = DownloadString.DownL(url, tryCount: 5);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//ul[contains(@class, 'search-container')]/li[contains(@class, 'search-item') and contains(@class, 'clearfix')]") ??
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
            var href =
                (n.SelectSingleNode(".//h4[contains(@class, 'search-title')]/a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://zakupki.lenreg.ru{href}";
            var purName = n.SelectSingleNode(".//h4[contains(@class, 'search-title')]/a")?.InnerText?.Trim() ??
                          throw new Exception(
                              $"cannot find purName in {href}");
            var purNum = purName.GetDataFromRegex(@"^([\w-]+)\s+").DelAllWhitespace();
            if (string.IsNullOrEmpty(purNum))
            {
                purNum = href.GetDataFromRegex(@"Index/(\d+)");
            }

            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var datePub = DateTime.Today;
            var dateEndT =
                n.SelectSingleNode(".//label[contains(., 'Срок окончания подачи оферт:')]/following-sibling::label")
                    ?.InnerText
                    ?.Trim().DelDoubleWhitespace() ?? throw new Exception(
                    $"cannot find dateEndT in {href}");
            var dateEnd = dateEndT.ParseDateUnRus();
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var dateUpdT =
                n.SelectSingleNode(".//p[@class = 'search-desc']")
                    ?.InnerText
                    ?.Trim().DelDoubleWhitespace() ?? "";

            dateUpdT = dateUpdT.GetDataFromRegex(@"^(.+\d{2}:\d{2})");
            var dateUpd = dateUpdT.ParseDateUnRus();
            if (dateUpd == DateTime.MinValue)
            {
                dateUpd = DateTime.Now;
            }

            var tn = new TenderLenReg("Электронный Магазин Ленинградской области", "https://zakupki.lenreg.ru/", 343,
                new TypeLenReg
                {
                    PurName = purName.ReplaceHtmlEntyty(), PurNum = purNum, DatePub = datePub, Href = href,
                    DateEnd = dateEnd,
                    DateUpd = dateUpd
                });
            ParserTender(tn);
        }
    }
}