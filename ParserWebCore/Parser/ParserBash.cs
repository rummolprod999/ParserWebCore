using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ParserWebCore.BuilderApp;
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
            for (var i = 0; i < _countPage; i++)
            {
                try
                {
                    GetPage(i);
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
                ["sec-ch-ua"] = "\" Not;A Brand\";v=\"99\", \"Google Chrome\";v=\"97\", \"Chromium\";v=\"97\"",
                ["accept"] = "application/json, text/plain, */*",
                ["sec-ch-ua-mobile"] = "?0",
                ["x-atmo"] = "jwYvNqVVWG4WjmP6GxnnzubwWZyMddyc",
                ["user-agent"] =
                    "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:100.0) Gecko/20100101 Firefox/100.0",
                ["origin"] = "https://zakaz.bashkortostan.ru",
            };

            var url =
                $"https://api-zakaz.bashkortostan.ru/apifront/purchases?filter=%7B%22purchaseCategories%22:[],%22conditionname%22:%22%22,%22orderType%22:null,%22customer%22:%22%22,%22regNumber%22:%22%22,%22orderDateStart%22:null,%22orderDateFinish%22:null,%22priceStartFrom%22:null,%22priceStartTo%22:null%7D&status=1&page={num}";
            var result = DownloadString.DownLHttpPostWithCookiesB2b(url, cookie: null, useProxy: AppBuilder.UserProxy,
                headers: headers);
            if (string.IsNullOrEmpty(result))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    url);
                return;
            }

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
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, t.ToString());
                }
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