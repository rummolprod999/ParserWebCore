#region

using System;
using System.Linq;
using System.Reflection;
using AngleSharp.Dom;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserKorabel : ParserAbstract, IParser
    {
        private const int Count = 6;

        public void Parsing()
        {
            Parse(ParsingKorabel);
        }

        private void ParsingKorabel()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"https://www.korabel.ru/trade/main/all_tenders/{i}.html?group=0&cid=0";
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
            var s = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var parser = new HtmlParser();
            var document = parser.Parse(s);
            var tens = document.All.Where(m => m.Id == "pag_data").Children("tr");
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
            var urlT = (t.QuerySelector("a")?.GetAttribute("href") ?? "").Trim();
            if (string.IsNullOrEmpty(urlT))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    url);
            }

            var tenderUrl = urlT;
            var purName = (t.QuerySelector("a")?.TextContent ?? "").Trim();
            if (string.IsNullOrEmpty(purName))
            {
                Log.Logger(
                    $"Empty string purName in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    tenderUrl);
            }

            var datePubT = (t.QuerySelector("td:nth-child(1)")?.TextContent ?? "").Trim();
            var datePub = datePubT.ParseDateUn("dd/MM/yyyyHH:mm");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger(
                    $"Empty dates in {GetType().Name}.{MethodBase.GetCurrentMethod().Name} datePubT: {datePubT}",
                    tenderUrl);
                return;
            }

            var dateEndT = (t.QuerySelector("td:nth-child(5)")?.TextContent ?? "").Trim();
            var dateEndExtract = dateEndT.GetDataFromRegex(@"(\d{2}/\d{2}/\d{4})");
            var dateEnd = dateEndExtract.ParseDateUn("dd/MM/yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var cusName = (t.QuerySelector("td:nth-child(3)")?.TextContent ?? "").Trim();
            var purNum = tenderUrl.GetDataFromRegex(@"/(\d+)\.html");

            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger(
                    $"Empty string purNum in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    tenderUrl);
            }

            var region = (t.QuerySelector("td:nth-child(4)")?.TextContent ?? "").Trim();
            var delivTerm = (t.QuerySelector("td:nth-child(6)")?.TextContent ?? "").Trim();
            var tn = new TenderKorabel("Корабел.ру", "https://www.korabel.ru/", 328,
                new TypeKorabel
                {
                    Href = tenderUrl,
                    DatePub = datePub,
                    DateEnd = dateEnd,
                    PurName = purName,
                    PurNum = purNum,
                    CusName = cusName,
                    Region = region,
                    DelivTerm = delivTerm
                });
            ParserTender(tn);
        }
    }
}