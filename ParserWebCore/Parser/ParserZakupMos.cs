using System;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;

namespace ParserWebCore.Parser
{
    public class ParserZakupMos : ParserAbstract, IParser
    {
        private readonly int _countPage = 20;
        private readonly string _url = "https://old.zakupki.mos.ru/api/Cssp/Purchase/PostQuery";

        public void Parsing()
        {
            Parse(ParsingZakupMos);
        }

        private void ParsingZakupMos()
        {
            for (int i = 0; i < _countPage; i++)
            {
                try
                {
                    GetPage(i);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
                }
            }
        }

        private void GetPage(int num)
        {
            var data =
                "{\"filter\":{\"auctionSpecificFilter\":{},\"needSpecificFilter\":{},\"tenderSpecificFilter\":{}},\"order\":[{\"field\":\"PublishDate\",\"desc\":true}],\"withCount\":true,\"take\":50,\"skip\":" +
                num * 50 + "}";
            var s = DownloadString.DownLZakMos(_url, data);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    _url);
                return;
            }

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
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, t.ToString());
                }
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var href = "";
            var purNum = "";
            var iD = 0;
            var needId = (int?) t.SelectToken("needId") ?? 0;
            var tenderId = 0;
            var auctionId = 0;
            if (needId != 0)
            {
                iD = needId;
                href = $"https://old.zakupki.mos.ru/#/need/{needId}";
                purNum = ((string) t.SelectToken("number") ?? "").Trim();
            }
            else
            {
                tenderId = (int?) t.SelectToken("tenderId") ?? 0;
                if (tenderId != 0)
                {
                    iD = tenderId;
                    href = $"https://old.zakupki.mos.ru/#/tenders/{tenderId}";
                    purNum = ((string) t.SelectToken("number") ?? "").Trim();
                }
            }

            if (href == "" || purNum == "")
            {
                auctionId = (int?) t.SelectToken("auctionId") ?? 0;
                if (auctionId != 0)
                {
                    iD = auctionId;
                    href = $"https://zakupki.mos.ru/auction/{auctionId}";
                    purNum = ((string) t.SelectToken("number") ?? "").Trim();
                }
            }

            if (href == "" || purNum == "")
            {
                Log.Logger("href or purNum is empty", t.ToString());
                return;
            }

            
            var purName = ((string) t.SelectToken("name") ?? "").Trim();
            var pubDateS = (string) t.SelectToken("beginDate") ?? "";
            var endDateS = (string) t.SelectToken("endDate") ?? "";
            var datePub = pubDateS.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            var dateEnd = endDateS.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            if (datePub == DateTime.MinValue && dateEnd == DateTime.MinValue)
            {
                Log.Logger("empty dates", t.ToString());
                return;
            }
            var status = ((string) t.SelectToken("stateName") ?? "").Trim();
            var regionName = ((string) t.SelectToken("regionName") ?? "").Trim();
            var orgName = ((string) t.SelectToken("purchaseCreator.name") ?? "").Trim();
            var orgInn = ((string) t.SelectToken("purchaseCreator.inn") ?? "").Trim();
            var nmck = (decimal?) t.SelectToken("startPrice") ?? 0.0m;
            Console.WriteLine(regionName);
            Console.WriteLine(nmck);
            Console.WriteLine();
        }
    }
}