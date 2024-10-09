using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
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

        private Dictionary<BashType, int> types = new Dictionary<BashType, int>()
        {
            { BashType.t44, 8 },
            { BashType.t223, 1 },
            { BashType.com, 1 },
            { BashType.req, 1 },
        };

        public void Parsing()
        {
            Parse(ParsingBash);
        }

        private void ParsingBash()
        {
            foreach (var t in types.Keys)
            {
                for (var i = 1; i <= types[t]; i++)
                {
                    try
                    {
                        GetPage(i, t);
                        Thread.Sleep(10000);
                    }
                    catch (Exception e)
                    {
                        Log.Logger($"Error in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                            e);
                    }
                }
            }
        }

        private void GetPage(int num, BashType bashType)
        {
            var headers = new Dictionary<string, string>
            {
                ["user-agent"] =
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/538.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/538.36",
                ["origin"] = "https://zakaz.bashkortostan.ru",
                ["referer"] = "https://zakaz.bashkortostan.ru/"
            };

            var url = "";
            switch (bashType)
            {
                case BashType.t44:
                    url = $"https://api-zakaz.bashkortostan.ru/api/front/v1/notices/fl44?status=accepting&page={num}";
                    break;
                case BashType.t223:
                    url = $"https://api-zakaz.bashkortostan.ru/api/front/v1/notices/fl223?status=accepting&page={num}";
                    break;
                case BashType.com:
                    url =
                        $"https://api-zakaz.bashkortostan.ru/api/front/v1/notices/commercial?status=accepting&page={num}";
                    break;
                case BashType.req:
                    url =
                        $"https://api-zakaz.bashkortostan.ru/api/front/v1/notices/quotation_requests?status=accepting&page={num}";
                    break;
            }

            var result = DownloadString.DownLHttpPostWithCookiesB2b(url, cookie: null, useProxy: AppBuilder.UserProxy,
                headers: headers);
            if (string.IsNullOrEmpty(result))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
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
                        ParserTenderObj(t, bashType);
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

        private void ParserTenderObj(JToken t, BashType bashType)
        {
            var id = ((string)t.SelectToken("id") ?? throw new ApplicationException("id not found")).Trim();
            var purName =
                ((string)(t.SelectToken(
                     "$..purchase_object")) ??
                 throw new ApplicationException($"purName not found {id}")).Trim();
            var purNum =
                ((string)(t.SelectToken(
                     "$..registration_number")) ??
                 throw new ApplicationException($"purNum not found {id}")).Trim();
            var datePub =
                ((DateTime?)(t.SelectToken(
                     "$..purchase_publish_date")) ??
                 throw new ApplicationException($"datePub not found {id}"));
            var dateEnd =
                ((DateTime?)(t.SelectToken(
                     "$..proposal_accept_end_date")) ??
                 throw new ApplicationException($"dateEnd not found {id}"));
            var href = "";
            switch (bashType)
            {
                case BashType.t44:
                    href = $"https://zakaz.bashkortostan.ru/purchases/grouped/fl44/item/{id}/view";
                    break;
                case BashType.t223:
                    href = $"https://zakaz.bashkortostan.ru/purchases/fl223/{id}/show";
                    break;
                case BashType.com:
                    href = $"https://zakaz.bashkortostan.ru/purchases/commercial/{id}/show";
                    break;
                case BashType.req:
                    href = $"https://zakaz.bashkortostan.ru/quotation-requests/{id}/show";
                    break;
            }

            var nmck =
                ((string)(t.SelectToken(
                     "$..starting_price")) ??
                 "").Trim();
            var status =
                ((string)(t.SelectToken(
                     "$..status.title")) ??
                 "").Trim();
            var dateContract =
                ((DateTime?)(t.SelectToken(
                     "$..planned_date_contract_fulfilled")) ??
                 DateTime.MinValue);
            var delivPlace =
                ((string)(t.SelectToken(
                     "$..delivery_addresses[0]")) ??
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