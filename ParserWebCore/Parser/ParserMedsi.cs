using System;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserMedsi : ParserAbstract, IParser
    {
        private readonly int _countPage = 3;
        private readonly string _url = "https://medsi.ru/ajax/ajax_purchases.php";

        public void Parsing()
        {
            Parse(ParsingMedsi);
        }

        private void ParsingMedsi()
        {
            for (var i = 1; i < _countPage; i++)
            {
                try
                {
                    GetPage(i);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
                }
            }
        }

        private void GetPage(int num)
        {
            var s = DownloadString.DownLMedsi(_url, num);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    _url);
                return;
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
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e);
                }
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var id = ((string) t.SelectToken("id") ?? "").Trim();
            var href = ((string) t.SelectToken("detail_page_url") ?? "").Trim();
            href = $"https://medsi.ru{href}";
            var purName = ((string) t.SelectToken("type_of_service") ?? "").Trim();
            var pubDateT = ((string) t.SelectToken("publish_date") ?? "").Trim();
            var endDateT = ((string) t.SelectToken("finish_date") ?? "").Trim();
            var datePub = pubDateT.ParseDateUn("dd.MM.yyyy");
            var dateEnd = endDateT.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            var orgContact = ((string) t.SelectToken("organizer[0]") ?? "").Trim();
            var tn = new TenderMedsi("Медицинская корпорация МЕДСИ", "https://medsi.ru/", 190,
                new TypeMedsi
                {
                    Href = href,
                    PurNum = id,
                    DatePub = datePub,
                    DateEnd = dateEnd,
                    PurName = purName,
                    OrgContact = orgContact
                });
            ParserTender(tn);
        }
    }
}