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
    public class ParserAtiSu : ParserAbstract, IParser
    {
        private readonly string url = "https://ati.su/gw/tenders/public/v1/tenders/search";

        public void Parsing()
        {
            Parse(ParsingAtiSu);
        }

        private void ParsingAtiSu()
        {
            for (var i = 0; i <= 10; i++)
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
                $"\"https://ati.su/gw/tenders/public/v1/tenders/search\" \\\n  -H \"content-type: application/json\" \\\n  --data-raw \"{{\"tender_statuses\":[\"1\",\"2\"],\"take\":10,\"skip\":{10 * i},\"order_by\":0}}\" \\\n  --compressed";
            var s = CurlDownloadSportMaster.DownL(data);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    url);
            }

            var jObj = JObject.Parse(s);
            var tenders = GetElements(jObj, "tenders");
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
            var id = ((string)t.SelectToken("tender.id") ?? "").Trim();
            var pubDate = (DateTime?)t.SelectToken("tender.phases[0].start_date") ?? DateTime.Now;
            var endDate = (DateTime?)t.SelectToken("tender.phases[0].end_date") ?? DateTime.Now;
            var purName = ((string)t.SelectToken("tender.cargo_type_name") ?? "").Trim();
            var nmck = ((string)t.SelectToken("tender.cashless_with_vat") ?? "").Trim();
            var tn = new TenderAtiSu("ATI.SU", "https://ati.su/", 385,
                new AtiSu
                {
                    Href = "https://ati.su/tenders/" + id,
                    PurNum = id,
                    DatePub = pubDate,
                    DateEnd = endDate,
                    PurName = purName,
                    Nmck = nmck,
                    BiddingDate = DateTime.Now
                });
            ParserTender(tn);
        }
    }
}