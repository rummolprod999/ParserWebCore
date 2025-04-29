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
    public class ParserRubex : ParserAbstract, IParser
    {
        private readonly string _urlpage = "https://rubexgroup.ru/zakupki/";

        public void Parsing()
        {
            Parse(ParsingRubex);
        }

        private void ParsingRubex()
        {
            try
            {
                ParsingPage(_urlpage);
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
                    "//div[@class = 'tenders-item']") ??
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

            var purName = n.SelectSingleNode(".//div[@class = 'tenders-item__text']")?.InnerText?.Trim() ??
                          throw new Exception(
                              $"cannot find purName in {href}");
            purName = HttpUtility.HtmlDecode(purName);
            var fullDateNum = n.SelectSingleNode(".//div[@class = 'tenders-item__title']")?.InnerText?.Trim() ??
                              throw new Exception(
                                  $"cannot find fullDateNum in {href}");
            var purNum = fullDateNum.GetDataFromRegex(@"Извещение\s+№\s+(\d+)");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum " + purName);
                return;
            }

            var datePubT = fullDateNum.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            if (string.IsNullOrEmpty(datePubT))
            {
                Log.Logger("Empty datePubT " + purName);
                return;
            }

            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEnd = DateTime.Now;
            var tn = new TenderRubex("RubEx Group", "https://rubexgroup.ru/", 248,
                new TypeRubex
                    { PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd });
            ParserTender(tn);
        }
    }
}