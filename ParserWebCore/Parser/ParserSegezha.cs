using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSegezha : ParserAbstract, IParser
    {
        private const int Count = 50;

        public void Parsing()
        {
            Parse(ParsingSegezha);
        }

        private void ParsingSegezha()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"https://old.segezha-group.com/purchasing/?PAGEN_1={i}";
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
                htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'zakupki-table')]/table/tbody/tr") ??
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

            href = $"https://old.segezha-group.com{href}";
            var purNum = href.GetDataFromRegex(@"purchasing/(\d+)/");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var purName = n.SelectSingleNode(".//a/span[1]")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find purName in {href}");
            var datePubT =
                (n.SelectSingleNode("./td[1]")
                    ?.InnerText ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd/MM/yy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT1 =
                (n.SelectSingleNode("./td[2]/text()")
                    ?.InnerText ?? "").Trim();
            var dateEndT2 =
                (n.SelectSingleNode("./td[2]/span")
                    ?.InnerText ?? "").Trim();
            var dateEndT = $"{dateEndT1} {dateEndT2}";
            var dateEnd = dateEndT.ParseDateUn("dd/MM/yy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                //return;
            }

            var status = (n.SelectSingleNode("./td[7]")
                ?.InnerText ?? "").Trim().GetDataFromRegex(@"(.+)\n");

            var cusName = (n.SelectSingleNode("./td[4]")
                ?.InnerText ?? "").Trim();
            var orgName = (n.SelectSingleNode("./td[5]")
                ?.InnerText ?? "").Trim();
            var tn = new TenderSegezha("ГК Сегежа", "https://segezha-group.com", 97,
                new TypeSegezha
                {
                    CusName = cusName, DateEnd = dateEnd, DatePub = datePub, Href = href, OrgName = orgName,
                    PurName = purName, PurNum = purNum, Status = status
                });
            ParserTender(tn);
        }
    }
}