#region

using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserOcontract : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingOcontract);
        }

        private void ParsingOcontract()
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    ParsingPage(i);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(int page)
        {
            var url =
                $"https://api.onlc.ru/purchases/v1/public/procedures?sort=-id&total=true&limit=100&offset={page * 100}&include=owner&filters%5BsearchType%5D=2";
            var s = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var jObj = JObject.Parse(s);
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
                        e);
                }
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var id = ((string)t.SelectToken("id") ?? "").Trim();
            var href = $"https://onlinecontract.ru/tenders/{id}";
            var currency = ((string)t.SelectToken("currency") ?? "").Trim();
            var name = ((string)t.SelectToken("name") ?? "").Trim();
            var nmck = ((string)t.SelectToken("startPrice") ?? "").Trim();
            var orgName = ((string)t.SelectToken("owner.name") ?? "").Trim();
            var status = ((string)t.SelectToken("status") ?? "").Trim();
            var placingWay = ((string)t.SelectToken("type") ?? "").Trim();
            var deliveryPlace = ((string)t.SelectToken("deliveryPlace") ?? "").Trim();
            var deliveryTerms = ((string)t.SelectToken("deliveryTerms") ?? "").Trim();
            var deliveryTime = ((string)t.SelectToken("deliveryTime") ?? "").Trim();
            var formOfPayment = ((string)t.SelectToken("formOfPayment") ?? "").Trim();
            var paymentTerms = ((string)t.SelectToken("paymentTerms") ?? "").Trim();
            var datePub = DateTime.Now;
            var endDate = (DateTime?)t.SelectToken("date") ?? DateTime.Now.AddDays(2);
            var biddingDateT = ((string)t.SelectToken("reBiddingStart") ?? "").Trim();
            var biddingDate = biddingDateT.ParseDateUn("dd.MM.yyyy HH:mm");
            var tn = new TenderOcontract("ЭТП ONLINECONTRACT", "http://onlinecontract.ru/tenders", 41,
                new TypeOcontract
                {
                    Href = href,
                    PurNum = id,
                    DatePub = datePub,
                    DateEnd = endDate,
                    PurName = name,
                    Status = status,
                    Nmck = nmck,
                    Currency = currency,
                    PlacingWay = placingWay,
                    BiddingDate = biddingDate,
                    DeliveryPlace = deliveryPlace,
                    DeliveryTerms = deliveryTerms,
                    DeliveryTime = deliveryTime,
                    FormOfPayment = formOfPayment,
                    PaymentTerms = paymentTerms,
                    OrgName = orgName
                });
            ParserTender(tn);
        }
    }
}