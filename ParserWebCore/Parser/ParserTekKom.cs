#region

using System;
using System.Linq;
using System.Reflection;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.SharedLibraries;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserTekKom : ParserAbstract, IParser
    {
        private int DateMinus => 3;

        public void Parsing()
        {
            Parse(ParsingTekKom);
        }

        private void ParsingTekKom()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * DateMinus * 24 * 60);
            var urlStart = $"https://www.tektorg.ru/223-fz/procedures?dpfrom={dateM:dd.MM.yyyy}";
            var max = 0;
            try
            {
                max = SharedTekTorg.GetCountPage(urlStart);
            }
            catch (Exception e)
            {
                Log.Logger(
                    $"Exception recieve count page in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    e, urlStart);
            }

            if (max == 0)
            {
                Log.Logger(
                    $"Null count page in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    urlStart);
                max = 1;
            }

            GetPage(max, urlStart);
        }

        private void GetPage(int max, string urlStart)
        {
            for (var i = 1; i <= max; i++)
            {
                var url = $"{urlStart}&page={i}&limit=500";
                try
                {
                    ParsingPage(url);
                }
                catch (Exception e)
                {
                    Log.Logger(
                        $"Exception in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                        e, urlStart);
                }
            }
        }

        private void ParsingPage(string url)
        {
            var s = DownloadString.DownLTektorg(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    url);
                return;
            }

            var parser = new HtmlParser();
            var document = parser.Parse(s);
            var tens = document.All.Where(m => m.ClassList.Contains("section-procurement__item") && m.TagName == "DIV");
            foreach (var t in tens)
            {
                try
                {
                    ParsingTender(t, url);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingTender(IElement t, string url)
        {
            var urlT = (t.QuerySelector("a.section-procurement__item-title")?.GetAttribute("href") ?? "").Trim();
            if (string.IsNullOrEmpty(urlT))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    url);
            }

            var tenderUrl = urlT;
            if (!urlT.Contains("https://"))
            {
                tenderUrl = $"https://www.tektorg.ru{urlT}";
            }

            var status = (t.QuerySelector("div span:contains('Статус:')")?.TextContent?.Replace("Статус:", "") ?? "")
                .Trim();
            if (status.Contains("Осталось:"))
            {
                status = status.GetDataFromRegex("(.+)\n.+Осталось:.+").Trim();
            }

            var datePubT =
                (t.QuerySelector("div.section-procurement__item-dateTo:contains('Дата публикации:')")?.TextContent ??
                 "").Replace("Дата публикации:", "").Trim();
            var dateEndT =
                (t.QuerySelector("div.section-procurement__item-dateTo:contains('Дата окончания приема заявок')")
                     ?.TextContent ??
                 "").Replace("Дата окончания приема заявок:", "").Replace("Дата окончания приема заявок", "").Trim();
            if (dateEndT == "")
            {
                dateEndT =
                    (t.QuerySelector("span:contains('Подведение итогов не позднее')")?.TextContent ??
                     "").Replace("Подведение итогов не позднее:", "").Trim();
            }

            if (dateEndT == "")
            {
                dateEndT =
                    (t.QuerySelector("div.section-procurement__item-dateTo:contains('Подведение итогов не позднее')")
                         ?.TextContent ??
                     "").Replace("Подведение итогов не позднее:", "").Replace("Подведение итогов не позднее", "")
                    .Trim();
            }

            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");
            if (datePub == DateTime.MinValue || dateEnd == DateTime.MinValue)
            {
                Log.Logger($"Empty dates in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    urlT, datePubT, dateEndT);
                return;
            }

            var purNumT = (t.QuerySelector("div.section-procurement__item-numbers > span")?.TextContent ?? "").Trim();
            var purNum = purNumT.Replace("Номер закупки на сайте ЭТП:", "").Trim();
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger($"Empty purNum in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    urlT, purNumT);
                return;
            }

            var tn = new TenderTekKom("ТЭК Торг Коммерческие закупки и 223-ФЗ",
                "https://www.tektorg.ru/223-fz/procedures", 138,
                new TypeTekKom
                {
                    Href = tenderUrl,
                    Status = status,
                    PurNum = purNum,
                    DatePub = datePub,
                    DateEnd = dateEnd
                });
            ParserTender(tn);
        }
    }
}