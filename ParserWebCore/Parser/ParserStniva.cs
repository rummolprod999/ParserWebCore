#region

using System;
using System.Linq;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserStniva : ParserAbstract, IParser
    {
        private readonly string _urlpage = "https://trade.stniva.ru/release.php";

        public void Parsing()
        {
            Parse(ParsingStniva);
        }

        private void ParsingStniva()
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
            var s = DownloadString.DownL(url, 5);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//table[contains(@class, 'table')]/tbody/tr") ??
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
            var href = (n.SelectSingleNode(".//td[6]//a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                href = _urlpage;
            }

            var purName = n.SelectSingleNode(".//td[5]")?.InnerText?.Trim() ??
                          throw new Exception(
                              $"cannot find purName in {href}");
            var purNum = n.SelectSingleNode(".//td[1]")?.InnerText?.Trim() ??
                         throw new Exception(
                             $"cannot find purNum in {href}");
            var datePub = DateTime.Today;
            var dEnd = n.SelectSingleNode(".//td[2]")?.InnerText?.Trim() ?? "";
            var tEnd = n.SelectSingleNode(".//td[3]")?.InnerText?.Trim() ?? "";
            var dateEndT = $"{dEnd} {tEnd}";
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var customers = (n.SelectNodes("./td[4]/span") ??
                             new HtmlNodeCollection(null)).Select(x => x?.InnerText.Trim() ?? "").ToList();
            var tn = new TenderStniva("ООО \"АПК \"Стойленская Нива\"", "https://trade.stniva.ru/", 342,
                new TypeStniva
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                    Customers = customers
                });
            ParserTender(tn);
        }
    }
}