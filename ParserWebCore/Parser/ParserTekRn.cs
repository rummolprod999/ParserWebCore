using System;
using System.Linq;
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
        private int _dateMinus => 10;

        public void Parsing()
        {
            Parse(ParsingTekRn);
        }

        private void ParsingTekRn()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * _dateMinus * 24 * 60);
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
                        $"Exception in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, urlStart);
                }
            }
        }

        private void ParsingPage(string url)
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
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    url);
            }

            var tenderUrl = urlT;
            if (urlT != null && !urlT.Contains("https://")) tenderUrl = $"https://www.tektorg.ru{urlT}";
            var status = (t.QuerySelector("div span:contains('Статус:')")?.TextContent?.Replace("Статус:", "") ?? "")
                .Trim();
            if (status.Contains("Осталось:"))
            {
                status = status.GetDataFromRegex("(.+)Осталось:.+").Trim();
            }
            var datePubT =
                (t.QuerySelector("div span:contains('Дата публикации:')")?.TextContent
                     ?.Replace("Дата публикации:", "") ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger($"Empty dates in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    tenderUrl, datePubT);
                return;
            }

            var purNum = (t.QuerySelector("div > span:contains('Номер закупки на сайте ЭТП:')")?.TextContent
                              ?.Replace("Номер закупки на сайте ЭТП:", "") ?? "").Trim();
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger($"Empty purNum in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    tenderUrl);
                return;
            }

            var tn = new TenderTekRn("ТЭК Торг ТЭК Роснефть", "https://www.tektorg.ru/rosneft/procedures", 149,
                new TypeTekRn
                {
                    Href = tenderUrl,
                    Status = status,
                    PurNum = purNum,
                    DatePub = datePub
                });
            ParserTender(tn);
        }
    }
}