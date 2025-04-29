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
    public class ParserSamolet : ParserAbstract, IParser
    {
        private readonly int _countPage = 20;

        public void Parsing()
        {
            Parse(ParsingMts);
        }

        private void ParsingMts()
        {
            for (var i = 1; i < _countPage; i++)
            {
                try
                {
                    GetPage(i);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                        e);
                }
            }
        }

        private void GetPage(int num)
        {
            var url =
                $"https://partner.samolet.ru/api/tender/tenders/?limit=15&page={num}";
            var result = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(result))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    url);
                return;
            }

            var jObj = JObject.Parse(result);
            var tenders = GetElements(jObj, "results");
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
            var id = ((string)t.SelectToken("id") ?? throw new ApplicationException("id not found")).Trim();
            var purName =
                ((string)t.SelectToken(
                     "name") ??
                 throw new ApplicationException($"purName not found {id}")).Trim();
            var purNum =
                ((string)t.SelectToken(
                    "shortName") ?? id).Trim();
            if (purNum.Trim() == "")
            {
                purNum = id;
            }

            var datePub = (DateTime?)t.SelectToken("created") ?? throw new ApplicationException("date pub not found");
            var dateEnd = (DateTime?)t.SelectToken("offersEndDt") ??
                          throw new ApplicationException("date end not found");
            var tender = new TypeSamolet
            {
                Href = $"https://partner.samolet.ru/tenders/{id}/info",
                PurNum = purNum,
                Id = id,
                PurName = purName,
                DatePub = datePub,
                DateEnd = dateEnd
            };
            ParserTender(new TenderSamolet("АО «Группа компаний «Самолет»", "https://samoletgroup.ru/", 130,
                tender));
        }
    }
}