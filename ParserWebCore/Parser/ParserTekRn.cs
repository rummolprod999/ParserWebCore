using System;
using System.Linq;
using System.Threading;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.SharedLibraries;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserTekRn : ParserAbstract, IParser
    {
        private int DateMinus => 30;

        public void Parsing()
        {
            Parse(ParsingTekRn);
            Parse(ParsingTekRnTkp);
        }

        private void ParsingTekRn()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * DateMinus * 24 * 60);
            var urlStart = $"https://www.tektorg.ru/rosneft/procedures?dpfrom={dateM:dd.MM.yyyy}";
            var max = 0;
            try
            {
                max = SharedTekTorg.GetCountPage(urlStart);
            }
            catch (Exception e)
            {
                Log.Logger(
                    $"Exception recieve count page in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    e, urlStart);
            }

            if (max == 0)
            {
                Log.Logger(
                    $"Null count page in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    urlStart);
                max = 1;
            }

            GetPage(max, urlStart);
        }

        private void ParsingTekRnTkp()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * DateMinus * 24 * 60);
            var urlStart = $"https://www.tektorg.ru/rosnefttkp/procedures?dpfrom={dateM:dd.MM.yyyy}";
            var max = 0;
            try
            {
                max = SharedTekTorg.GetCountPage(urlStart);
            }
            catch (Exception e)
            {
                Log.Logger(
                    $"Exception recieve count page in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    e, urlStart);
            }

            if (max == 0)
            {
                Log.Logger(
                    $"Null count page in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    urlStart);
                max = 1;
            }

            GetPage(max, urlStart, true);
        }

        private void GetPage(int max, string urlStart, bool tektkp = false)
        {
            for (var i = 1; i <= max; i++)
            {
                var url = $"{urlStart}&page={i}&limit=500";
                try
                {
                    ParsingPage(url, tektkp);
                }
                catch (Exception e)
                {
                    Log.Logger(
                        $"Exception in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, urlStart);
                }
            }
        }

        private void ParsingPage(string url, bool tektkp = false)
        {
            var s = DownloadString.DownL(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    url);
            }

            var parser = new HtmlParser();
            var document = parser.Parse(s);
            var tens = document.All.Where(m => m.ClassList.Contains("section-procurement__item") && m.TagName == "DIV");
            foreach (var t in tens)
            {
                try
                {
                    ParsingTender(t, url, tektkp);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingTender(IElement t, string url, bool tektkp = false)
        {
            Thread.Sleep(4000);
            var urlT = (t.QuerySelector("a.section-procurement__item-title")?.GetAttribute("href") ?? "").Trim();
            if (string.IsNullOrEmpty(urlT))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    url);
            }

            var purName = (t.QuerySelector("a.section-procurement__item-title")?.TextContent ??
                           "");

            var tenderUrl = urlT;
            if (!urlT.Contains("https://")) tenderUrl = $"https://www.tektorg.ru{urlT}";
            var status = (t.QuerySelector("div.section-procurement__item-dateTo:contains('Статус:')")?.TextContent
                    ?.Replace("Статус:", "") ?? "")
                .Trim();
            if (status.Contains("Осталось:"))
            {
                status = status.GetDataFromRegex("(.+)\n.+Осталось:.+").Trim();
            }

            var datePubT =
                (t.QuerySelector("div.section-procurement__item-dateTo:contains('Дата публикации процедуры:')")
                     ?.TextContent ??
                 "").Replace("Дата публикации процедуры:", "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger($"Empty dates in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    tenderUrl, datePubT);
                return;
            }

            var dateEndT =
                (t.QuerySelector(
                         "div.section-procurement__item-dateTo:contains('Дата окончания срока подачи технико-коммерческих частей:')")
                     ?.TextContent ??
                 "").Replace("Дата окончания срока подачи технико-коммерческих частей:", "").Trim();
            if (dateEndT == "")
            {
                dateEndT =
                    (t.QuerySelector(
                             "div.section-procurement__item-dateTo:contains('Дата окончания срока подачи коммерческих частей:')")
                         ?.TextContent ??
                     "").Replace("Дата окончания срока подачи коммерческих частей:", "").Trim();
            }

            if (dateEndT == "")
            {
                dateEndT =
                    (t.QuerySelector(
                             "div.section-procurement__item-dateTo:contains('Дата окончания срока подачи технических частей:')")
                         ?.TextContent ??
                     "").Replace("Дата окончания срока подачи технических частей:", "").Trim();
            }

            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger($"Empty dateEnd {tenderUrl}");
                dateEnd = dateEnd.AddDays(2);
            }

            var purNum = (t.QuerySelector("div > span:contains('Номер закупки на сайте ЭТП:')")?.TextContent
                ?.Replace("Номер закупки на сайте ЭТП:", "") ?? "").Trim();
            if (purNum == "")
            {
                purNum =
                    (t.QuerySelector("span:contains('Номер процедуры:')")?.TextContent ??
                     "").Replace("Номер процедуры:", "").Trim();
            }

            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger($"Empty purNum in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    tenderUrl);
                return;
            }

            var orgName = (t.QuerySelector("div > span:contains('Организатор:') + a")?.TextContent
                ?.Replace("Организатор:", "") ?? "").Trim();

            var dateScoringT =
                (t.QuerySelector("div.section-procurement__item-dateTo:contains('Подведение итогов не позднее:')")
                     ?.TextContent ??
                 "").Replace("Подведение итогов не позднее:", "").Trim();
            var dateScoring = dateScoringT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");

            var nmckT = (t.QuerySelector("div.section-procurement__item-totalPrice")?.TextContent ?? "").Trim();
            var nmck = nmckT.ExtractPriceNew();
            if (tektkp)
            {
                var tn = new TenderTekRn("ТЭК Торг ТЭК Роснефть Запросы (Т)КП",
                    "https://www.tektorg.ru/rosneft/procedures", 149,
                    new TypeTekRn
                    {
                        Href = tenderUrl,
                        Status = status,
                        PurNum = purNum,
                        DatePub = datePub,
                        Scoring = dateScoring,
                        OrgName = orgName,
                        DateEnd = dateEnd,
                        Nmck = nmck,
                        PurName = purName,
                    });
                ParserTender(tn);
            }
            else
            {
                var tn = new TenderTekRn("ТЭК Торг ТЭК Роснефть", "https://www.tektorg.ru/rosnefttkp/procedures", 149,
                    new TypeTekRn
                    {
                        Href = tenderUrl,
                        Status = status,
                        PurNum = purNum,
                        DatePub = datePub,
                        Scoring = dateScoring,
                        OrgName = orgName,
                        DateEnd = dateEnd,
                        Nmck = nmck,
                        PurName = purName,
                    });
                ParserTender(tn);
            }
        }
    }
}