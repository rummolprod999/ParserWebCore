using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserNazot : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingNazot);
        }

        private void ParsingNazot()
        {
            for (var i = 0; i <= 5; i++)
            {
                try
                {
                    GetPage(i);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void GetPage(int i)
        {
            var d = DateTime.Now.AddMonths(i).ToString("yyyy-MM");
            var href = $"https://n-azot.ru/tender.php?month={d}-01";
            var data =
                $"\"https://n-azot.ru/tender.php?month={d}-01\" -H \"User-Agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36\" --data-raw \"day=0&lang=RU&tec_date={d}-01\" --compressed";
            var s = CurlDownloadSportMaster.DownL(data);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}");
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes("//table[@class = 'tender']//tr[position() > 1]") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserTender(a, href);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTender(HtmlNode n, string href)
        {
            var rowspan = (n.SelectSingleNode("./td[@rowspan]")
                ?.InnerText ?? "").Trim();
            var num = string.IsNullOrEmpty(rowspan) ? 0 : 1;
            var datePub = DateTime.Now;
            var dateEndT =
                (n.SelectSingleNode($"./td[{num + 1}]")
                    ?.InnerText ?? "").Trim();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd");
                return;
            }

            var purName =
                (n.SelectSingleNode($"./td[{num + 3}]")
                    ?.InnerText ?? "").Trim();
            var purNum = purName.ToMd5();
            var notice1 = (n.SelectSingleNode($"./td[{num + 5}]")
                ?.InnerText ?? "").Trim();
            var notice2 = (n.SelectSingleNode($"./td[{num + 6}]")
                ?.InnerText ?? "").Trim();
            var notice = $"{notice1} {notice2}";
            var delivTerm1 = (n.SelectSingleNode($"./td[{num + 4}]")
                ?.InnerText ?? "").Trim();
            var delivTerm2 = (n.SelectSingleNode($"./td[{num + 2}]")
                ?.InnerText ?? "").Trim();
            var delivTerm = $"{delivTerm1} Срок исполнения заказа: {delivTerm2}";
            var tn = new TenderNazot("Химическая компания «Щекиноазот»", "http://n-azot.ru/", 391,
                new TypeNazot
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Notice = notice,
                    DelivTerm = delivTerm,
                    DateBind = DateTime.MinValue
                });
            ParserTender(tn);
        }
    }
}