using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserTpta : ParserAbstract, IParser
    {
        private const string Urlpage = "https://www.tpta.ru/suppliers/ten/";

        public void Parsing()
        {
            Parse(ParsingTpta);
        }

        private void ParsingTpta()
        {
            try
            {
                ParsingPage(Urlpage);
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
                    "//div[@class = 'lot']") ??
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
            var href = Urlpage;
            var purName =
                n.SelectSingleNode(".//p[contains(., 'Наименование:')]")?.InnerText?.Replace("Наименование:", "")
                    .Trim() ?? throw new Exception(
                    $"cannot find purName in {href}");
            var purNumAndDate =
                n.SelectSingleNode(".//p[contains(., 'Номер заявки:')]")?.InnerText?.Replace("Номер заявки:", "")
                    .Trim() ?? throw new Exception(
                    $"cannot find purNumAndDate in {href}");
            var purNum = purNumAndDate.GetDataFromRegex(@"(^[\d-]+)\s");
            if (purNum == "")
            {
                throw new Exception(
                    $"cannot find purNum in {href}");
            }

            var pubDateT = purNumAndDate.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var datePub = pubDateT.ParseDateUn("dd.MM.yyyy");
            var endDateT =
                n.SelectSingleNode(".//p[contains(., 'Дата окончания сбора заявок:')]")?.InnerText
                    ?.Replace("Дата окончания сбора заявок:", "")
                    .Trim() ?? throw new Exception(
                    $"cannot find endDateT in {href}");
            var dateEnd = endDateT.ParseDateUn("dd.MM.yyyy");
            var poObjects = n.SelectNodes(
                                ".//table/tbody/tr") ??
                            new HtmlNodeCollection(null);
            var pList = new List<TypeObjectTpta>();
            foreach (var poObject in poObjects)
            {
                var pName = poObject.SelectSingleNode("./td[1]")?.InnerText?.Trim() ?? "";
                var pOkei = poObject.SelectSingleNode("./td[4]")?.InnerText?.Trim() ?? "";
                var pQuant = poObject.SelectSingleNode("./td[3]")?.InnerText?.Trim() ?? "";
                if (pName != "")
                {
                    pList.Add(new TypeObjectTpta {Name = pName, Okei = pOkei, Quantity = pQuant});
                }
            }

            var tn = new TenderTpta("«Маркетинговые технологии»", "http://is-mt.pro/", 278,
                new TypeTpta
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                    ObjectsPurchase = pList
                });
            ParserTender(tn);
        }
    }
}