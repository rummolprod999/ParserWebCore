using System;
using System.Web;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserLotOnline : ParserAbstract, IParser
    {
        private readonly int _countPage = 10;

        public void Parsing()
        {
            Parse(ParsingLotOnline);
        }

        private void ParsingLotOnline()
        {
            for (var i = 0; i < _countPage; i++)
            {
                try
                {
                    GetPage(i * 20);
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
            var url =
                $"https://market.lot-online.ru/searchServlet?query={HttpUtility.UrlEncode($"{{\"types\":[\"RFI\",\"SMALL_PURCHASE\",\"ELECTRONIC_STORE\"]}}&filter={{\"state\":[\"ALL\"]}}&sort={{\"placementDate\":false}}&limit={{\"min\":{num},+\"max\":{num + 20}}}")}";
            var result = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(result))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
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
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e, t.ToString());
                }
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var id = ((string) t.SelectToken("identifier") ?? "").Trim();
            var purName = ((string) t.SelectToken("title") ?? "").Trim();
            var publicationDateT = ((string) t.SelectToken("gdStartDate") ?? "").Trim();
            var endDateT = ((string) t.SelectToken("gdEndDate") ?? "").Trim();
            var publicationDate = publicationDateT.ParseDateUn("dd.MM.yyyy HH:mm");
            var endDate = endDateT.ParseDateUn("dd.MM.yyyy HH:mm");
            var href = ((string) t.SelectToken("lotLink") ?? "").Trim();
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(href) || publicationDate == DateTime.MinValue ||
                endDate == DateTime.MinValue)
            {
                Log.Logger("bad tender", id);
                return;
            }

            var orgName = ((string) t.SelectToken("organizer.title") ?? "").Trim();
            var orgInn = ((string) t.SelectToken("organizer.inn") ?? "").Trim();
            var status = ((string) t.SelectToken("state.title") ?? "").Trim();
            var regionName = ((string) t.SelectToken("regionCodes[0]") ?? "").Trim();
            var nmck = ((string) t.SelectToken("price") ?? "").Trim().DelAllWhitespace();
            nmck = nmck.GetDataFromRegex(">([\\d.]+)").DelAllWhitespace();
            var tender = new TypeLotOnline
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
                Nmck = nmck
            };
            ParserTender(new TenderLotOnline("АО «Российский аукционный дом»", "https://market.lot-online.ru/", 274,
                tender));
        }
    }
}