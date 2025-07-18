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
    public class ParserAcron : ParserAbstract, IParser
    {
        private readonly int _countPage = 10;
        private readonly string _orgName = "ПАО «Акрон»";

        public void Parsing()
        {
            Parse(ParsingAcron);
        }

        private void ParsingAcron()
        {
            for (var i = 0; i < _countPage; i++)
            {
                try
                {
                    GetPage(i * 20);
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
                $"https://etp.acron.ru/searchServlet?query=%7B%22types%22%3A%5B%22BUYING%22%5D%7D&filter=%7B%22state%22%3A%5B%22GD%22%5D%7D&sort=%7B%22placementDate%22%3Afalse%7D&limit=%7B%22min%22%3A{num}%2C%22max%22%3A{num + 20}%7D";
            var result = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(result))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    url);
                return;
            }

            var jObj = JObject.Parse(result);
            var tenders = GetElements(jObj, "list");
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
            var id = ((string)t.SelectToken("identifier") ?? "").Trim();
            var purName = ((string)t.SelectToken("title") ?? "").Trim();
            var publicationDateT = ((string)t.SelectToken("gdStartDate") ?? "").Trim();
            var endDateT = ((string)t.SelectToken("gdEndDate") ?? "").Trim();
            var publicationDate = publicationDateT.ParseDateUn("dd.MM.yyyy HH:mm");
            var endDate = endDateT.ParseDateUn("dd.MM.yyyy HH:mm");
            var href = ((string)t.SelectToken("lotLink") ?? "").Trim();
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(href) || publicationDate == DateTime.MinValue ||
                endDate == DateTime.MinValue)
            {
                Log.Logger("bad tender", id);
                return;
            }

            var orgName = ((string)t.SelectToken("organizer.title") ?? _orgName).Trim();
            var orgInn = ((string)t.SelectToken("organizer.inn") ?? string.Empty).Trim();
            var cusName = ((string)t.SelectToken("customer[0].title") ?? string.Empty).Trim();
            var cusInn = ((string)t.SelectToken("customer[0].inn") ?? string.Empty).Trim();
            var status = ((string)t.SelectToken("state.title") ?? "").Trim();
            var regionName = ((string)t.SelectToken("city[0]") ?? "").Trim();
            var pwName = ((string)t.SelectToken("placementType") ?? "").Trim();
            var nmck = ((string)t.SelectToken("price") ?? "").Trim().DelAllWhitespace();
            nmck = nmck.GetDataFromRegex(">([\\d.]+)").DelAllWhitespace();
            if (cusName == string.Empty || cusInn == string.Empty)
            {
                cusName = orgName;
                cusInn = orgInn;
            }

            var tender = new TypeEtpu
            {
                Href = href,
                PurNum = id,
                PurName = purName,
                DatePub = publicationDate,
                DateEnd = endDate,
                OrgName = orgName,
                OrgInn = orgInn,
                RegionName = regionName,
                Status = status,
                Nmck = nmck,
                PlacingWay = pwName,
                CusName = cusName,
                CusInn = cusInn
            };
            ParserTender(new TenderAcron("Электронная тендерная площадка ПАО «Акрон»",
                "https://etp.acron.ru/", 364,
                tender));
        }
    }
}