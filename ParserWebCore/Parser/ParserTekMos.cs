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
    public class ParserTekMos: ParserAbstract, IParser
    {
        private int _dateMinus => 35;

        public void Parsing()
        {
            Parse(ParsingTekMos);
        }

        private void ParsingTekMos()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * _dateMinus * 24 * 60);
            var urlStart = $"https://www.tektorg.ru/mosenergo/procedures?dpfrom={dateM:dd.MM.yyyy}";
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
            var status = (t.QuerySelector("div span:contains('Статус:')")?.TextContent?.Replace("Статус:", "") ?? "").Trim();
            status = status.GetDataFromRegex("(.+)Осталось:.+").Trim();
            var tn = new TenderTekMos("ТЭК Торг ТЭК Мосэнерго", "https://www.tektorg.ru/mosenergo/procedures", 140,
                new TypeTekMos
                {
                    Href = tenderUrl,
                    Status = status
                });
            ParserTender(tn);
        }
    }
}