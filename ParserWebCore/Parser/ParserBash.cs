using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json.Linq;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserBash : ParserAbstract, IParser
    {
        private readonly int _countPage = 20;

        public void Parsing()
        {
            Parse(ParsingBash);
        }

        private void ParsingBash()
        {
            for (var i = 1; i < _countPage; i++)
            {
                try
                {
                    GetPage(i);
                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e);
                }
            }
        }

        private void GetPage(int num)
        {
            var headers = new Dictionary<string, string>
            {
                ["authority"] = "api-zakaz.bashkortostan.ru",
                ["sec-ch-ua"] = "\"Not_A Brand\";v=\"99\", \"Google Chrome\";v=\"109\", \"Chromium\";v=\"109\"",
                ["accept"] = "application/json, text/plain, */*",
                ["user-agent"] =
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/538.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/538.36",
                ["origin"] = "https://zakaz.bashkortostan.ru",
                ["referer"] = "https://zakaz.bashkortostan.ru/",
                ["accept-language"] = "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7"
            };

            var url =
                $"https://api-zakaz.bashkortostan.ru/apifront/purchases?page={num}";
            var result = DownloadString.DownLHttpPostWithCookiesB2b(url, cookie: null, useProxy: true,
                headers: headers);
            if (string.IsNullOrEmpty(result))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    url);
                return;
            }

            try
            {
                var jObj = JObject.Parse(result);
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
            catch (Exception e)
            {
                Log.Logger("result " + result);
                throw;
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var id = ((string)t.SelectToken("id") ?? throw new ApplicationException("id not found")).Trim();
            var purName =
                ((string)(t.SelectToken(
                     "$..purchase_name")) ??
                 throw new ApplicationException($"purName not found {id}")).Trim();
            var purNum =
                ((string)(t.SelectToken(
                     "$..reg_number")) ??
                 throw new ApplicationException($"purNum not found {id}")).Trim();
            var datePub =
                ((DateTime?)(t.SelectToken(
                     "$..purchase_start")) ??
                 throw new ApplicationException($"datePub not found {id}"));
            var dateEnd =
                ((DateTime?)(t.SelectToken(
                     "$..purchase_end")) ??
                 throw new ApplicationException($"dateEnd not found {id}"));
            var href = $"https://zakaz.bashkortostan.ru/purchases/{id}/order-info";
            var nmck =
                ((string)(t.SelectToken(
                     "$..start_price")) ??
                 "").Trim();
            var status =
                ((string)(t.SelectToken(
                     "$..status")) ??
                 "").Trim();
            var dateContract =
                ((DateTime?)(t.SelectToken(
                     "$..planned_contract_date")) ??
                 DateTime.MinValue);
            var delivPlace =
                ((string)(t.SelectToken(
                     "$..delivery[0].address")) ??
                 "").Trim();
            var tender = new TypeBash
            {
                Href = href,
                PurNum = purNum,
                PurName = purName,
                DatePub = datePub,
                DateEnd = dateEnd,
                Nmck = nmck,
                Status = status,
                ContractDate = dateContract,
                DelivPlace = delivPlace,
                Id = id
            };
            ParserTender(new TenderBash("Агрегатор торгов малого объема Республики Башкортостан",
                "https://zakaz.bashkortostan.ru/", 354,
                tender));
        }
    }
}