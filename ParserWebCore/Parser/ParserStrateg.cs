#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserStrateg : ParserAbstract, IParser
    {
        private readonly List<string> _urls = new List<string>
        {
            "https://strateg-etp.ru/admin/ExportSeldon.aspx"
        };

        public void Parsing()
        {
            Parse(ParsingStrateg);
        }

        private void ParsingStrateg()
        {
            try
            {
                foreach (var url in _urls)
                {
                    GetPage(url);
                }
            }
            catch (Exception e)
            {
                Log.Logger($"Error in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}", e);
            }
        }

        private void GetPage(string url)
        {
            var s = DownloadString.DownL1251(url);
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
                    Log.Logger($"Error in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                        e);
                }
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var purNum = ((string)t.SelectToken("@NoticeNumb") ?? "").Trim();
            var href = ((string)t.SelectToken("@Link") ?? "").Trim();
            var purName = ((string)t.SelectToken("@Name") ?? "").Trim();
            var pubDateT = ((string)t.SelectToken("@PurchaseStart") ?? "").Trim();
            var datePub = pubDateT.ParseDateUn("dd.MM.yyyy");
            var endDateT = ((string)t.SelectToken("@PurchaseFinishDate") ?? "").Trim();
            var endDate = endDateT.ParseDateUn("dd.MM.yyyy");
            var status = ((string)t.SelectToken("@PurchaseStatus") ?? "").Trim();
            var tn = new TenderStrateg("ЭТП Стратег", "https://strateg-etp.ru/", 302,
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