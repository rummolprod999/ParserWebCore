using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserAkashevo : ParserAbstract, IParser
    {
        private const int Count = 10;

        public void Parsing()
        {
            Parse(ParsingAkashevo);
        }

        private void ParsingAkashevo()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"http://tender.akashevo.ru/purchase/?PAGEN_1={i}";
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
            var s = DownloadString.DownL(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes("//div[@class = 't_lots']/table/tbody/tr") ??
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
            var href = (n.SelectSingleNode(".//a[contains(@class, 't_lot_title')]")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"http://tender.akashevo.ru/purchase/{href}";
            var purNum = href.GetDataFromRegex(@"LOT_ID=(\d+)");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var purName = (n.SelectSingleNode(".//a[contains(@class, 't_lot_title')]")
                ?.InnerText ?? "").Trim();
            var orgName = (n.SelectSingleNode(".//span[b = 'Компания:']")
                ?.InnerText ?? "").Replace("Компания:", "").Trim();
            var contactPerson = (n.SelectSingleNode(".//span[b = 'Ответственный:']")
                ?.InnerText ?? "").Replace("Ответственный:", "").Trim();
            var phone = (n.SelectSingleNode(".//span[b = 'Телефон:']")
                ?.InnerText ?? "").Replace("Телефон:", "").Trim();
            var datePubT =
                (n.SelectSingleNode(".//span[contains(b, 'Дата начала:')]")
                    ?.InnerText ?? "").Replace("Дата начала:", "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode(".//span[contains(b, 'Дата окончания:')]")
                    ?.InnerText ?? "").Replace("Дата окончания:", "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                return;
            }

            var tn = new TenderAgrokomplex("Птицефабрика Акашевская", "http://tender.akashevo.ru/purchase/", 98,
                new TypeAgrokomplex
                {
                    OrgName = orgName,
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    ContactPerson = contactPerson,
                    Phone = phone
                });
            ParserTender(tn);
        }
    }
}