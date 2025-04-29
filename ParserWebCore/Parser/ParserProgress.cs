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
    public class ParserProgress : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingProgress);
        }

        private void ParsingProgress()
        {
            for (var i = 1; i < 25; i++)
            {
                var urlStart =
                    $"https://tender.progressagro.com/api/Lots/anonymous/{i}/25/dateStart/desc?globalFilter=actual";
                var s = DownloadString.DownL(urlStart);
                var jObj = JObject.Parse(s);
                var tenders = GetElements(jObj, "items");
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
        }

        private void ParserTenderObj(JToken t)
        {
            var id = ((string)t.SelectToken("id") ?? throw new ApplicationException("id not found")).Trim();
            var placingWay = ((string)t.SelectToken(
                                  "typeName") ??
                              "").Trim();
            var status = ((string)t.SelectToken(
                              "statusName") ??
                          "").Trim();
            var purName = ((string)t.SelectToken(
                               "name") ??
                           throw new ApplicationException($"purName not found {id}")).Trim();
            var datePub =
                (DateTime?)t.SelectToken(
                    "$..dateStart") ??
                throw new ApplicationException($"datePub not found {id}");
            var dateEnd =
                (DateTime?)t.SelectToken(
                    "$..dateEnd") ??
                throw new ApplicationException($"dateEnd not found {id}");
            var tn = new TenderProgress("«Прогресс Агро»",
                "https://tender.progressagro.com", 388,
                new TypeProgress
                {
                    Href = "https://tender.progressagro.com/lots/detail/" + id,
                    Status = status,
                    DatePub = datePub,
                    DateEnd = dateEnd,
                    PurName = purName,
                    PurNum = id,
                    PwName = placingWay
                });
            ParserTender(tn);
        }
    }
}