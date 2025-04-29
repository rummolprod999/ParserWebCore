#region

using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserGpb : ParserAbstract, IParser
    {
        private readonly string _url = "https://etp.gpb.ru/nsi/kimapi/publicpriceorderlist?limit=50&page=";
        private int _countPage;

        public void Parsing()
        {
            Parse(ParsingGpb);
        }

        private void ParsingGpb()
        {
            Parse("https://etp.gpb.ru/nsi/kimapi/publicpriceorderlist");
            Parse($"{_url}1");
            for (var i = 2; i <= _countPage; i++)
            {
                Parse($"{_url}{i}");
            }
        }

        private void Parse(string _url)
        {
            var s = DownloadString.DownLUserAgent(_url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    _url);
                return;
            }

            var jObj = JObject.Parse(s);
            var totalCount = (int?)jObj.SelectToken("totalCount") ?? 0;
            if (_countPage == 0)
            {
                _countPage = totalCount / 50 + 1;
            }

            var tenders = GetElements(jObj, "entries");
            foreach (var t in tenders)
            {
                try
                {
                    ParserTenderObj(t);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                        e, t.ToString());
                }
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var id = ((string)t.SelectToken("id") ?? "").Trim();
            var purName = ((string)t.SelectToken("title") ?? "").Trim();
            var pubDateS = (string)t.SelectToken("date_sent") ?? "";
            var datePub = pubDateS.ParseDateUn("yyyy-MM-dd HH:mm:sszz");
            var endDateS = (string)t.SelectToken("date_response") ?? "";
            var dateEnd = endDateS.ParseDateUn("yyyy-MM-dd HH:mm:sszz");
            var typeT = new TypeGpb
            {
                Href = $"https://etp.gpb.ru/nsi/kimapi/publicpriceorderinfo?id={id}",
                PurNum = id,
                DatePub = datePub,
                DateEnd = dateEnd,
                PurName = purName
            };
            ParserTender(new TenderGpb("ЭТП ГПБ", "https://etp.gpb.ru/", 301,
                typeT));
        }
    }
}