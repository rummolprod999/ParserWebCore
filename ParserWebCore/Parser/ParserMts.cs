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
    public class ParserMts : ParserAbstract, IParser
    {
        private readonly int _countPage = 20;

        public void Parsing()
        {
            Parse(ParsingMts);
        }

        private void ParsingMts()
        {
            for (var i = 0; i < _countPage; i++)
            {
                try
                {
                    GetPage(i);
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
                $"https://tenders.mts.ru/api/v2/tender?searchQuery=&pageSize=5&page={num}&attributesForSort=tenders_publication_date,desc";
            var result = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(result))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
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
                    Log.Logger($"Error in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                        e, t.ToString());
                }
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var id = ((string)t.SelectToken("id") ?? throw new ApplicationException("id not found")).Trim();
            var purName =
                ((string)t.SelectToken(
                     "name") ??
                 throw new ApplicationException($"purName not found {id}")).Trim();
            var publicationDate = DateTime.Now;
            var dateEndS =
                ((string)t.SelectToken(
                     "endDateAcceptingOffers") ??
                 publicationDate.AddDays(2).ToString("yyyy-MM-dd")).Trim();
            var endDate = dateEndS.ParseDateUn("yyyy-MM-dd");
            var purNum =
                ((string)t.SelectToken(
                    "number") ?? id).Trim();
            if (purNum.Trim() == "")
            {
                purNum = id;
            }

            var status =
                ((string)t.SelectToken(
                    "status") ?? "").Trim();
            var pwName = "";
            var region =
                ((string)t.SelectToken(
                    "regions") ?? "").Trim();
            var tender = new TypeMts
            {
                Href = $"https://tenders.mts.ru/tenders/{id}",
                PurNum = purNum,
                Id = id,
                PurName = purName,
                DatePub = publicationDate,
                DateEnd = endDate,
                Region = region,
                Status = status,
                PlacingWay = pwName
            };
            ParserTender(new TenderMts("ПАО «Мобильные ТелеСистемы»", "https://tenders.mts.ru/", 131,
                tender));
        }
    }
}