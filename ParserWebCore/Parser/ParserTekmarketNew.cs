using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserTekmarketNew : ParserAbstract, IParser
    {
        private readonly string url = "https://www.tektorg.ru/api/getProcedures";

        private List<TenderTekMarketNew> _tendersList = new List<TenderTekMarketNew>();
        private int DateMinus => 3;

        public void Parsing()
        {
            Parse(ParsingTekMarket);
        }

        private void ParsingTekMarket()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * DateMinus * 24 * 60);
            var urlStart = $"https://www.tektorg.ru/market/procedures?dpfrom={dateM:dd.MM.yyyy}";
            var s = DownloadString.DownL(urlStart);
            var buildid = s.GetDataFromRegex("\"buildId\":\"(\\d+)\"");

            for (var i = 1; i <= 20; i++)
            {
                var data =
                    $"{{\"params\":{{\"sectionsCodes[0]\":\"market\",\"dpfrom\":\"{dateM:dd.MM.yyyy}\",\"page\":{i},\"sort\":\"actual\"}}}}";
                try
                {
                    ParsingPage(data, buildid);
                }
                catch (Exception e)
                {
                    Log.Logger(
                        $"Exception in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, url);
                }
            }

            foreach (var tenderTekMarketNew in _tendersList)
            {
                try
                {
                    ParserTender(tenderTekMarketNew);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(string data, string buildid)
        {
            var s = DownloadString.DownLZakMos(url, data);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    url);
            }

            var jObj = JObject.Parse(s);
            var tenders = GetElements(jObj, "data");
            foreach (var t in tenders)
            {
                try
                {
                    ParserTenderObj(t, buildid);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, t.ToString());
                }
            }
        }

        private void ParserTenderObj(JToken t, string buildid)
        {
            var id = ((string)t.SelectToken("id") ?? throw new ApplicationException("id not found")).Trim();
            var tenderUrl = $"https://www.tektorg.ru/_next/data/{buildid}/ru/market/procedures/{id}.json?id={id}";
            var status = ((string)(t.SelectToken(
                              "statusName")) ??
                          "").Trim();
            var purName = ((string)(t.SelectToken(
                               "title")) ??
                           throw new ApplicationException($"purName not found {id}")).Trim();
            var datePub =
                ((DateTime?)(t.SelectToken(
                     "$..datePublished")) ??
                 throw new ApplicationException($"datePub not found {id}"));
            var dateEnd =
                ((DateTime?)(t.SelectToken(
                     "$..dateEndRegistration")) ??
                 throw new ApplicationException($"dateEnd not found {id}"));
            var purNum = ((string)(t.SelectToken(
                              "registryNumber")) ??
                          throw new ApplicationException($"purNum not found {id}")).Trim();
            var pwName = ((string)(t.SelectToken(
                              "typeName")) ??
                          "").Trim();
            var tn = new TenderTekMarketNew("Электронная торговая площадка ТЭК-Торг Секция малых и срочных закупок",
                "https://www.tektorg.ru/market/procedures", 384,
                new TypeTekMarket
                {
                    Href = "https://www.tektorg.ru/market/procedures/" + id,
                    Status = status,
                    DatePub = datePub,
                    DateEnd = dateEnd,
                    PurName = purName,
                    PurNum = purNum,
                    PwName = pwName,
                    Down = tenderUrl
                });
            _tendersList.Add(tn);
        }
    }
}