#region

using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserRzdMed : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingRzdMed);
        }

        private void ParsingRzdMed()
        {
            for (var i = 1; i <= 10; i++)
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
            var data =
                $"\"https://zakupki.rzd-medicine.ru/api/purchase/orders?limit=100&page={i}\" -H \"Accept: application/json\" -H \"Content-Type: application/json\" -H \"User-Agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36'\" --compressed";
            var s = CurlDownloadSportMaster.DownL(data);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}");
            }

            var jObj = JObject.Parse(s);
            var tenders = GetElements(jObj, "data");
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
            var pubDate = (DateTime?)t.SelectToken("created") ?? DateTime.Now;
            var endDate = (DateTime?)t.SelectToken("application_deadline") ?? DateTime.Now;
            var purName = ((string)t.SelectToken("name") ?? "").Trim();
            var status = ((string)t.SelectToken("status.name") ?? "").Trim();
            var tn = new TenderRzdMed("«РЖД-Медицина»", "https://zakupki.rzd-medicine.ru/", 386,
                new RzdMed
                {
                    Href = "https://zakupki.rzd-medicine.ru/",
                    PurNum = id,
                    DatePub = pubDate,
                    DateEnd = endDate,
                    PurName = purName,
                    Status = status,
                    token = t
                });
            ParserTender(tn);
        }
    }
}