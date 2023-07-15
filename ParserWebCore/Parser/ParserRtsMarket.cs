using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserRtsMarket : ParserAbstract, IParser
    {
        private readonly string _apiUrl = "https://zmo-new-webapi.rts-tender.ru/api/Trade/GetTradesForAnonymous";
        private readonly int _countPage = 60;

        private readonly Dictionary<string, int>[] _sections =
        {
            new Dictionary<string, int>
            {
                ["{\"Title\":\"Тип поиска\",\"ShortName\":\"t\",\"Type\":1,\"Value\":1,\"Name\":\"MarketSearchAction\"},{\"Title\":\"Признак малого и среднего предпринимательства\",\"ShortName\":\"smp\",\"Type\":1,\"Value\":0,\"Name\":\"SmpFilterState\"}"]
                    = 3
            }
        };

        public void Parsing()
        {
            Parse(ParsingRtsMarket);
        }

        private void ParsingRtsMarket()
        {
            _sections.ToList().ForEach(x =>
            {
                for (var i = 1; i < _countPage; i++)
                {
                    try
                    {
                        GetPage(i, in x);
                    }
                    catch (Exception e)
                    {
                        Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                            e);
                    }
                }
            });
        }

        private void GetPage(int num, in Dictionary<string, int> section)
        {
            var data =
                $"{{\"page\":{num},\"itemsPerPage\":10,\"tradeState\":\"\",\"OnlyTradesWithMyApplications\":false,\"filterPriceMin\":\"\",\"filterPriceMax\":\"\",\"filterDateFrom\":null,\"filterDateTo\":null,\"filterFillingApplicationEndDateFrom\":null,\"FilterFillingApplicationEndDateTo\":null,\"filterTradeEasuzNumber\":\"\",\"showOnlyOwnTrades\":true,\"IsImmediate\":false,\"IsExpire\":false,\"UsedClassificatorType\":5,\"classificatorCodes\":[],\"CustomerFullNameOrInn\":\"\",\"CustomerAddress\":\"\",\"ParticipantHasApplicationsOnTrade\":\"\",\"UseCustomerInn\":false,\"UseCustomerName\":true,\"ZmoFzType\":0,\"ZmoFinanceSourceBudget\":true,\"ZmoFinanceSourceOutOfBudget\":true,\"ZmoFinanceSourceMixedBudget\":true,\"ZmoFinanceSourceMunicipalBudget\":false,\"ZmoFinanceSourceRegionalBudget\":false,\"ZmoFinanceSourceFederalBudget\":false,\"TradeSearchType\":50}}";
            var s = DownloadString.DownLRtsZmo(_apiUrl, data, section.Values.First());
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    _apiUrl);
                return;
            }

            if (s.ToLower().Contains("exception"))
            {
                Log.Logger(s);
                return;
            }

            var jObj = JObject.Parse(s);
            var tenders = GetElements(jObj, "invdata");
            foreach (var t in tenders)
            {
                try
                {
                    ParserTenderObj(t, section.Values.First());
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, t.ToString());
                }
            }
        }

        private void ParserTenderObj(JToken t, int sec)
        {
            var id = ((string)t.SelectToken("Id") ?? "").Trim();
            var lotId = ((string)t.SelectToken("LotId") ?? "").Trim();
            var purName = ((string)t.SelectToken("TradeName") ?? "").Trim();
            var nmck = ((string)t.SelectToken("InitialPrice") ?? "").Trim();
            var cusName = ((string)t.SelectToken("CustomerFullName") ?? "").Trim();
            var stateString = ((string)t.SelectToken("TradeStateName") ?? "").Trim();
            var publicationDate = (DateTime?)t.SelectToken("PublicationDate") ?? DateTime.MinValue;
            var endDate = (DateTime?)t.SelectToken("FillingApplicationEndDate") ?? DateTime.MinValue;
            var delivPlaces = GetElements(t, "DeliveryKladrs")
                .Select(m => ((string)m.SelectToken("Name") ?? "").Trim()).ToArray();
            var host = GetElements(t, "Hosts").Select(m => (string)m ?? "").FirstOrDefault();
            if (string.IsNullOrEmpty(id) || publicationDate == DateTime.MinValue ||
                endDate == DateTime.MinValue)
            {
                Log.Logger("tender id or dates is empty", id);
                return;
            }

            var tender = new TypeZmoRts
            {
                Id = id, LotId = lotId, CusName = cusName, DeliveryKladrRegionName = delivPlaces, EndDate = endDate,
                Host = host, Nmck = nmck, PublicationDate = publicationDate, PurName = purName,
                StateString = stateString
            };
            ParserTender(new TenderRtsMarket("ЭЛЕКТРОННЫЙ МАГАЗИН РТС-МАРКЕТ", "https://zmo.rts-tender.ru/", 95,
                tender, sec));
        }
    }
}