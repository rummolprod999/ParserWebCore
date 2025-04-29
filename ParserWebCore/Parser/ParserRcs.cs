#region

using System;
using System.Collections.Generic;
using System.Globalization;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserRcs : ParserAbstract, IParser
    {
        private const string Urlpage = "http://rcs-e.ru/category/zakupki";

        public void Parsing()
        {
            Parse(ParsingRcs);
        }

        private void ParsingRcs()
        {
            try
            {
                ParsingPage($"{Urlpage}");
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
                    "//div[@class = 'tablezakupki']//tr[not(@style)]") ??
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
            var href = (n.SelectSingleNode(".//td[8]//a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            var purNum = (n.SelectSingleNode(".//td[2]")?.InnerText ?? "")
                .Replace("ПДО №", "").Trim();
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var region = (n.SelectSingleNode(".//td[1]")?.InnerText ?? "")
                .Trim();
            var nmck = (n.SelectSingleNode(".//td[6]")?.InnerText ?? "")
                .Trim().ExtractPriceNew();
            var status = (n.SelectSingleNode(".//td[7]")?.InnerText ?? "")
                .Trim();
            var purName = (n.SelectSingleNode(".//td[5]")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var datePubT =
                n.SelectSingleNode(".//td[3]")
                    ?.InnerText ?? "";
            var myCultureInfo = new CultureInfo("ru-RU");
            var datePub = DateTime.Parse(datePubT, myCultureInfo);
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode(".//td[4]")
                    ?.InnerText ?? "").Replace("(", "").Replace(")", "").Trim();
            var dateEnd = DateTime.Parse(dateEndT, myCultureInfo);
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                dateEnd = datePub.AddDays(2);
            }

            var attachments = new List<TypeSamCom.Attachment>();
            var atts = n.SelectNodes(@".//td[8]//a");
            foreach (var a in atts)
            {
                var name = a.InnerText.Trim();
                var url = a.GetAttributeValue("href", "").Trim();
                if (name == "" || url == "")
                {
                    continue;
                }

                attachments.Add(new TypeSamCom.Attachment { Name = name, Url = url });
            }

            var tn = new TenderRcs("ООО «РКС-Инжиниринг»", "http://rcs-e.ru/", 338,
                new TypeRcs
                {
                    Status = status,
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Region = region,
                    Nmck = nmck,
                    Attachments = attachments
                });
            ParserTender(tn);
        }
    }
}