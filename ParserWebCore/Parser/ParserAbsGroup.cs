#region

using System;
using System.Web;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserAbsGroup : ParserAbstract, IParser
    {
        private readonly string _startPage = "https://tender.absgroup.ru/tenders/?PAGEN_1=";
        private const int maxPage = 5;

        public void Parsing()
        {
            Parse(ParsingMaxi);
        }

        private void ParsingMaxi()
        {
            try
            {
                ParsingPage();
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }
        }

        private void ParsingPage()
        {
            for (var i = 1; i <= maxPage; i++)
            {
                ParsingPage($"{_startPage}{i}");
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
                    "//div[@class = 'table tenders']//div[@class = 'table__row pure-u-1']") ??
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
                (n.SelectSingleNode(".//div[contains(@class,'tenders__cell_name')]/a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://tender.absgroup.ru{href}";
            var purName = n.SelectSingleNode(".//div[contains(@class,'tenders__cell_name')]/a")?.InnerText?.Trim() ??
                          throw new Exception(
                              $"cannot find purName in {href}");
            purName = HttpUtility.HtmlDecode(purName);
            var purNum = href.ToMd5();
            var status =
                n.SelectSingleNode(".//div[contains(@class,'tenders__cell_to status_')]/span")?.InnerText?.Trim() ?? "";
            var datePubT =
                n.SelectSingleNode(".//div[contains(@class,'tenders__cell_from')]/span")?.InnerText?.Trim() ??
                throw new Exception(
                    $"cannot find datePubT in {href}");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEndT = n.SelectSingleNode(".//div[contains(@class,'tenders__cell_to pure-u')]/span")?.InnerText
                               ?.Trim() ??
                           throw new Exception(
                               $"cannot find dateEndT in {href}");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            var tn = new TenderAbsGroup("АБСОЛЮТ ИНВЕСТИЦИОННАЯ ГРУППА", "https://tender.absgroup.ru/", 288,
                new TypeAbsGroup
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurName = purName,
                    PurNum = purNum,
                    Status = status
                });
            ParserTender(tn);
        }
    }
}