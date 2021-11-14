using System;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

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
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e);
                }
            }
        }

        private void GetPage(int num)
        {
            var url =
                $"https://tenders.mts.ru/api/v1/tenders?pageSize=20&page={num}&isSubscribe=false&attributesForSort=tenders_publication_date,desc";
            var result = DownloadString.DownLUserAgent(url);
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
                     "$..attributeCategories[0].attributes[?(@.name == 'Название закупки')].value")) ??
                 throw new ApplicationException($"purName not found {id}")).Trim();
            var datePubS =
                ((string)(t.SelectToken(
                     "$..attributeCategories[0].attributes[?(@.name == 'Дата публикации')].value")) ??
                 throw new ApplicationException($"datePubS not found {id}")).Trim();
            var publicationDate = datePubS.ParseDateUn("yyyy-MM-dd");
            var dateEndS =
                ((string)(t.SelectToken(
                     "$..attributeCategories[0].attributes[?(@.name == 'Дата окончания приема предложений')].value")) ??
                 publicationDate.AddDays(2).ToString("yyyy-MM-dd")).Trim();
            var endDate = dateEndS.ParseDateUn("yyyy-MM-dd");
            var purNum =
                ((string)(t.SelectToken(
                     "$..attributeCategories[0].attributes[?(@.name == 'Номер закупки в OeBS')].value")) ??
                 (string)(t.SelectToken(
                     "$..attributeCategories[0].attributes[?(@.name == 'Номер закупки на tenders')].value")) ??
                 (string)(t.SelectToken(
                     "$..attributeCategories[0].attributes[?(@.name == 'ID закупки в OeBS')].value")) ?? id).Trim();
            if (purNum.Trim() == "")
            {
                purNum = id;
            }

            var status =
                ((string)(t.SelectToken(
                    "$..attributeCategories[0].attributes[?(@.name == 'Статус закупки')].value.value")) ?? "").Trim();
            var pwName =
                ((string)(t.SelectToken(
                    "$..attributeCategories[0].attributes[?(@.name == 'Статус закупки')].value.value")) ?? "").Trim();
            var region =
                ((string)(t.SelectToken(
                    "$..attributeCategories[0].attributes[?(@.name == 'Регион')].value[0].value")) ?? "").Trim();
            var tender = new TypeMts
            {
                Href = $"https://tenders.mts.ru/tenders/{id}",
                PurNum = purNum,
                PurName = purName,
                DatePub = publicationDate,
                DateEnd = endDate,
                Region = region,
                Status = status,
                PlacingWay = pwName,
            };
            ParserTender(new TenderMts("ПАО «Мобильные ТелеСистемы»", "https://tenders.mts.ru/", 131,
                tender));
        }
    }
}