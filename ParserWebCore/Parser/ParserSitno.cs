using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSitno : ParserAbstract, IParser
    {
        private const int Count = 5;

        public void Parsing()
        {
            Parse(ParsingSitno);
        }

        private void ParsingSitno()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"http://sitno.ru/tender/?PAGEN_1={i}";
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
            var s = DownloadString.DownL1251(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes("//ul[@class = 'tenders_list']/li[@class = 'tenders_item']") ??
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
            var href = (n.SelectSingleNode(".//a[contains(@class, 'tit_link')]")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"http://sitno.ru/tender/{href}";
            var purNum = href.GetDataFromRegex(@"LOT_ID=(\d+)");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var purName = (n.SelectSingleNode(".//a[contains(@class, 'tit_link')]")
                ?.InnerText ?? "").Trim();
            var orgName = (n.SelectSingleNode(".//p[b = 'Компания:']")
                ?.InnerText ?? "").Replace("Компания:", "").Trim();
            var contactPerson = (n.SelectSingleNode(".//p[b = 'Ответственный:']")
                ?.InnerText ?? "").Replace("Ответственный:", "").Trim();
            var phone = (n.SelectSingleNode(".//p[b = 'Телефон:']")
                ?.InnerText ?? "").Replace("Телефон:", "").Trim();
            var datePubT =
                (n.SelectSingleNode(".//p[contains(b, 'Дата начала:')]")
                    ?.InnerText ?? "").Replace("Дата начала:", "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                (n.SelectSingleNode(".//p[contains(b, 'Дата окончания:')]")
                    ?.InnerText ?? "").Replace("Дата окончания:", "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                return;
            }

            var tn = new TenderAgrokomplex("Компания СИТНО", "http://sitno.ru/tender/", 103,
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