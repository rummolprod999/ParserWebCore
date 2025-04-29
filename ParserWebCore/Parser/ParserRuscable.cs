#region

using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserRuscable : ParserAbstract, IParser
    {
        private const string _url = "https://tenders.ruscable.ru/?page=";

        public void Parsing()
        {
            Parse(ParsingRuscable);
        }

        private void ParsingRuscable()
        {
            for (var i = 1; i < 5; i++)
            {
                try
                {
                    ParsingPage($"{_url}{i}");
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(string url)
        {
            var s = DownloadString.DownL1251(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//div[@class = 'tender-list']//div[contains(@class, 'node')]") ??
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
            var href = (n.SelectSingleNode(".//a[@class = 'title']")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            var purName = n.SelectSingleNode(".//div[@class = 'title-text']")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find purName in {_url}");
            var purNum = purName.GetDataFromRegex(@"Тендер\s+№\s+(\d+)");
            var datePubT = n.SelectSingleNode(".//div[@class = 'date-create']")?.InnerText?.Trim() ??
                           throw new Exception(
                               $"cannot find datePubT in {_url}");
            datePubT = datePubT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", purName);
                return;
            }

            var endDateT = n.SelectSingleNode(".//div[@class = 'date']")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find endDateT in {_url}");
            endDateT = endDateT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var dateEnd = endDateT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var status = n.SelectSingleNode(".//div[@class = 'city']/preceding-sibling::div")
                ?.GetAttributeValue("class", "")?.Trim() ?? "";

            var delivPlace = n.SelectSingleNode(".//div[@class = 'city']")?.InnerText?.Trim() ?? "";
            var addInfo = n.SelectSingleNode(".//div[@class = 'description']")?.InnerText?.Trim() ?? "";
            var purObj = new List<TypeRuscable.PurObj>();
            var po = n.SelectNodes(@".//table[@class = 'positions']/tbody/tr");
            foreach (var p in po)
            {
                var name = p.SelectSingleNode("./td[1]")?.InnerText?.Trim() ?? "";
                var qc = p.SelectSingleNode("./td[2]")?.InnerText?.Trim() ?? "";
                var quant = qc.GetDataFromRegex(@"([\d ,]+)").Replace(",", ".");
                var okei = qc.GetDataFromRegex(@"\s+(.+)$");
                purObj.Add(new TypeRuscable.PurObj { Name = name, Quant = quant, Okei = okei });
            }

            var tn = new TenderRuscable("«РусКабель»", "https://tenders.ruscable.ru/", 396,
                new TypeRuscable
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    DelivPlace = delivPlace,
                    purObj = purObj,
                    DelivAddInfo = addInfo,
                    Status = status
                });
            ParserTender(tn);
        }
    }
}