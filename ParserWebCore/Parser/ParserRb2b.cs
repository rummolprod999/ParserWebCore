using System;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserRb2b : ParserAbstract, IParser
    {
        private readonly string _url = "https://kkz1885.rb2b.ru/admin/ExportSeldon.aspx";

        public void Parsing()
        {
            Parse(ParsingRb2b);
        }

        private void ParsingRb2b()
        {
            try
            {
                GetPage();
            }
            catch (Exception e)
            {
                Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
            }
        }

        private void GetPage()
        {
            var s = DownloadString.DownL1251(_url);
            var doc = new XmlDocument();
            doc.LoadXml(s);
            var jsons = JsonConvert.SerializeXmlNode(doc);
            var json = JObject.Parse(jsons);
            var tenders = GetElements(json, "source.tender");
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
            var purNum = ((string) t.SelectToken("@NoticeNumb") ?? "").Trim();
            var href = ((string) t.SelectToken("@Link") ?? "").Trim();
            var purName = ((string) t.SelectToken("@Name") ?? "").Trim();
            var pubDateT = ((string) t.SelectToken("@PurchaseStart") ?? "").Trim();
            var datePub = pubDateT.ParseDateUn("dd.MM.yyyy");
            var endDateT = ((string) t.SelectToken("@PurchaseFinishDate") ?? "").Trim();
            var endDate = endDateT.ParseDateUn("dd.MM.yyyy");
            var status = ((string) t.SelectToken("@PurchaseStatus") ?? "").Trim();
            var tn = new TenderRb2b("RB2B Электронная торговая площадка", "https://zakupki.rb2b.ru/", 291,
                new TypeRb2b
                {
                    Href = href,
                    Status = status,
                    PurNum = purNum,
                    DatePub = datePub,
                    DateEnd = endDate,
                    PurName = purName,
                    JsonT = t
                });
            ParserTender(tn);
        }
    }
}