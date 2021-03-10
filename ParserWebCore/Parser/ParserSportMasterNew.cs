using System;
using Newtonsoft.Json.Linq;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSportMasterNew : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingSportMaster);
        }

        private void ParsingSportMaster()
        {
            for (var i = 1; i < 4; i++)
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
            var arguments =
                $"\"https://zakupki.sportmaster.ru/bitrix/services/main/ajax.php?action=itm%3Atender.api.tender.getList\" \\\n  -H \"Connection: keep-alive\" \\\n  -H \"sec-ch-ua: \"Google Chrome\";v=\"89\", \"Chromium\";v=\"89\", \";Not A Brand\";v=\"99\"\" \\\n  -H \"Content-Type: application/x-www-form-urlencoded\" \\\n  -H \"sec-ch-ua-mobile: ?0\" \\\n  -H \"User-Agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.72 Safari/537.36\" \\\n  -H \"Bx-ajax: true\" \\\n  -H \"Accept: */*\" \\\n  -H \"Origin: https://zakupki.sportmaster.ru\" \\\n  -H \"Sec-Fetch-Site: same-origin\" \\\n  -H \"Sec-Fetch-Mode: cors\" \\\n  -H \"Sec-Fetch-Dest: empty\" \\\n  -H \"Referer: https://zakupki.sportmaster.ru/tender_list.php\" \\\n  -H \"Accept-Language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7\" \\\n  -H \"Cookie: PHPSESSID=uro0ehgn9ams02p8io0fkltapm; BITRIX_SM_UIDH=1a26fb8cbbb2ace5b4155697a8cf36d6; BITRIX_SM_UIDL=info_enter-it_ru; BITRIX_SM_SALE_UID=0; BITRIX_SM_LOGIN=info_enter-it_ru; BITRIX_SM_SOUND_LOGIN_PLAYED=Y\" \\\n  --data-raw \"order[id]=desc&offset={page}&creatorId=0&&SITE_ID=s1&sessid=540be8180303bf2ab76434753039b279&SITE_TEMPLATE_ID=sportmaster\"";
            var docString =
                CurlDownloadSportMaster.DownL(arguments);
            var jObj = JObject.Parse(docString);
            var tenders = GetElements(jObj, "data.tenders");
            foreach (var t in tenders)
            {
                try
                {
                    ParserTenderObj(t);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e);
                }
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var id = ((string) t.SelectToken("id") ?? "").Trim();
            var detailUrl = ((string) t.SelectToken("detailUrl") ?? "").Trim();
            var href = $"https://zakupki.sportmaster.ru{detailUrl}";
            var pwName = ((string) t.SelectToken("typeName") ?? "").Trim();
            var purName = ((string) t.SelectToken("name") ?? "").Trim();
            var pubDate = (DateTime?) t.SelectToken("startDate") ?? DateTime.Now;
            var endDate = (DateTime?) t.SelectToken("endDate") ?? DateTime.Now;
            var person = ((string) t.SelectToken("responsibleName") ?? "").Trim();
            var phoneEmail = ((string) t.SelectToken("responsiblePhone") ?? "").Trim();
            var tn = new TenderSportMasterNew("ООО «Спортмастер»", "http://zakupki.sportmaster.ru/", 216,
                new TypeSport1
                {
                    Href = href,
                    PurNum = id,
                    DatePub = pubDate,
                    DateEnd = endDate,
                    PurName = purName,
                    PwName = pwName,
                    Person = person,
                    PhoneEmail = phoneEmail
                });
            ParserTender(tn);
        }
    }
}