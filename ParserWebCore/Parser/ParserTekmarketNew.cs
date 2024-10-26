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
            _tendersList.Clear();
            Parse(ParsingRosneft);
            _tendersList.Clear();
            Parse(ParsingRosneftTkp);
        }

        private void ParsingTekMarket()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * DateMinus * 24 * 60);
            var urlStart = $"https://www.tektorg.ru/market/procedures?dpfrom={dateM:dd.MM.yyyy}";
            var s = DownloadString.DownL(urlStart);
            var buildid = s.GetDataFromRegex("\"buildId\":\"(\\w+)\"");

            for (var i = 1; i <= 20; i++)
            {
                var data =
                    $"{{\"params\":{{\"sectionsCodes[0]\":\"market\",\"dpfrom\":\"{dateM:dd.MM.yyyy}\",\"page\":{i},\"sort\":\"actual\"}}}}";
                try
                {
                    ParsingPage(data, buildid, 0);
                }
                catch (Exception e)
                {
                    Log.Logger(
                        $"Exception in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, url);
                }
            }

            for (var i = 1; i <= 20; i++)
            {
                var data =
                    $"{{\"params\":{{\"sectionsCodes[0]\":\"market\",\"page\":{i},\"sort\":\"datePublished_desc\"}}}}";
                try
                {
                    ParsingPage(data, buildid, 0);
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

        private void ParsingRosneft()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * DateMinus * 24 * 60);
            var urlStart = $"https://www.tektorg.ru/rosneft/procedures?dpfrom={dateM:dd.MM.yyyy}";
            var s = DownloadString.DownL(urlStart);
            var buildid = s.GetDataFromRegex("\"buildId\":\"(\\w+)\"");

            for (var i = 1; i <= 20; i++)
            {
                var data =
                    $"{{\"params\":{{\"sectionsCodes[0]\":\"rosneft\",\"dpfrom\":\"{dateM:dd.MM.yyyy}\",\"page\":{i},\"sort\":\"actual\"}}}}";
                try
                {
                    ParsingPage(data, buildid, 1);
                }
                catch (Exception e)
                {
                    Log.Logger(
                        $"Exception in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, url);
                }
            }

            for (var i = 1; i <= 20; i++)
            {
                var data =
                    $"{{\"params\":{{\"sectionsCodes[0]\":\"rosneft\",\"page\":{i},\"sort\":\"datePublished_desc\"}}}}";
                try
                {
                    ParsingPage(data, buildid, 1);
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
        
        private void ParsingRosneftTkp()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * DateMinus * 24 * 60);
            var urlStart = $"https://www.tektorg.ru/rosnefttkp/procedures?dpfrom={dateM:dd.MM.yyyy}";
            var s = DownloadString.DownL(urlStart);
            var buildid = s.GetDataFromRegex("\"buildId\":\"(\\w+)\"");

            for (var i = 1; i <= 20; i++)
            {
                var data =
                    $"{{\"params\":{{\"sectionsCodes[0]\":\"rosnefttkp\",\"dpfrom\":\"{dateM:dd.MM.yyyy}\",\"page\":{i},\"sort\":\"actual\"}}}}";
                try
                {
                    ParsingPage(data, buildid, 2);
                }
                catch (Exception e)
                {
                    Log.Logger(
                        $"Exception in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, url);
                }
            }

            for (var i = 1; i <= 20; i++)
            {
                var data =
                    $"{{\"params\":{{\"sectionsCodes[0]\":\"rosnefttkp\",\"page\":{i},\"sort\":\"datePublished_desc\"}}}}";
                try
                {
                    ParsingPage(data, buildid, 2);
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

        private void ParsingPage(string data, string buildid, int i)
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
                    ParserTenderObj(t, buildid, i);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, t.ToString());
                }
            }
        }

        private void ParserTenderObj(JToken t, string buildid, int i)
        {
            var id = ((string)t.SelectToken("id") ?? throw new ApplicationException("id not found")).Trim();
            var tenderUrl = "";
            if (i == 0)
            {
                tenderUrl = $"https://www.tektorg.ru/_next/data/{buildid}/ru/market/procedures/{id}.json?id={id}";
            }
            else if (i == 1)
            {
                tenderUrl = $"https://www.tektorg.ru/_next/data/{buildid}/ru/rosneft/procedures/{id}.json?id={id}";
            }
            else if (i == 2)
            {
                tenderUrl = $"https://www.tektorg.ru/_next/data/{buildid}/ru/rosnefttkp/procedures/{id}.json?id={id}";
            }

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
            var dateBid =
                ((DateTime?)(t.SelectToken(
                     "$..dateEndRegistrationCom")) ??
                 DateTime.MinValue);
            var dateScor =
                ((DateTime?)(t.SelectToken(
                     "$..dateStartRegistrationCom")) ??
                 DateTime.MinValue);
            var dateEnd =
                ((DateTime?)(t.SelectToken(
                     "$..dateEndRegistration")) ?? (DateTime?)(t.SelectToken(
                        "$..dateRegistrationTech")) ??
                    datePub.AddDays(2));
            var purNum = ((string)(t.SelectToken(
                              "registryNumber")) ??
                          throw new ApplicationException($"purNum not found {id}")).Trim();
            var pwName = ((string)(t.SelectToken(
                              "typeName")) ??
                          "").Trim();
            var Href = "";
            if (i == 0)
            {
                Href = "https://www.tektorg.ru/market/procedures/" + id;
            }
            else if (i == 1)
            {
                Href = "https://www.tektorg.ru/market/procedures/" + id;
            }
            else if (i == 2)
            {
                Href = "https://www.tektorg.ru/rosneft/procedures/" + id;
            }
            var tt = new TypeTekMarket
            {
                Href = Href,
                Status = status,
                DatePub = datePub,
                DateEnd = dateEnd,
                PurName = purName,
                PurNum = purNum,
                PwName = pwName,
                Down = tenderUrl,
                DateBid = dateBid,
                DateScor = dateScor
            };
            if (i == 0)
            {
                var tn = new TenderTekMarketNew("Электронная торговая площадка ТЭК-Торг Секция малых и срочных закупок",
                    "https://www.tektorg.ru/market/procedures", 384, tt
                );
                _tendersList.Add(tn);
            }

            if (i == 1)
            {
                var tn = new TenderTekMarketNew("ТЭК Торг ТЭК Роснефть Запросы (Т)КП",
                    "https://www.tektorg.ru/rosneft/procedures", 362, tt
                );
                _tendersList.Add(tn);
            }
            if (i == 2)
            {
                var tn = new TenderTekMarketNew("ТЭК Торг ТЭК Роснефть",
                    "https://www.tektorg.ru/rosnefttkp/procedures", 149, tt
                );
                _tendersList.Add(tn);
            }
        }
    }
}