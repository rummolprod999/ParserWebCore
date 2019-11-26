using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSberB2B : ParserAbstract, IParser
    {
        private readonly int _countPage = 20;
        private readonly string _url = "https://sberb2b.ru/request/get-public-requests?r_published_at=desc";

        public void Parsing()
        {
            Parse(ParsingSber);
        }

        private void ParsingSber()
        {
            for (int i = 1; i < _countPage; i++)
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
            var s = DownloadString.DownLSber(_url, num);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    _url);
                return;
            }

            var jObj = JObject.Parse(s);
            var tenders = GetElements(jObj, "data.list");
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
            var href = $"https://sberb2b.ru/request/supplier/preview/{id}";
            var cusName = ((string) t.SelectToken("customer.short_name") ?? "").Trim();
            var purName = ((string) t.SelectToken("name") ?? "").Trim();
            var purNum = ((string) t.SelectToken("numeric_hash") ?? "").Trim();
            var pubDate = (DateTime?) t.SelectToken("published_at") ?? DateTime.Now;
            var endDate = (DateTime?) t.SelectToken("send_kp_until_at") ?? DateTime.Now;
            var status = ((string) t.SelectToken("public_request_status") ?? "").Trim();
            var tn = new TenderSberB2B("SberB2B", "https://sberb2b.ru/", 220,
                new TypeSber
                {
                    Href = href,
                    Status = status,
                    PurNum = purNum,
                    DatePub = pubDate,
                    DateEnd = endDate,
                    PurName = purName,
                    CusName = cusName
                });
            ParserTender(tn);
        }
    }
}