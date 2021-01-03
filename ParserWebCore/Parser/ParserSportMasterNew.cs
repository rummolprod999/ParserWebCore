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
                $"\"https://zakupki.sportmaster.ru/bitrix/services/main/ajax.php?action=itm%3Atender.api.tender.getList\" -H \"Connection: keep-alive\" -H \"sec-ch-ua: \\\"Chromium\\\";v=\\\"88\\\", \\\"Google Chrome\\\";v=\\\"88\\\", \\\";Not A Brand\\\";v=\\\"99\\\"\" -H \"Content-Type: application/x-www-form-urlencoded\" -H \"sec-ch-ua-mobile: ?0\" -H \"User-Agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.50 Safari/537.36\" -H \"Bx-ajax: true\" -H \"Accept: */*\" -H \"Origin: https://zakupki.sportmaster.ru\" -H \"Sec-Fetch-Site: same-origin\" -H \"Sec-Fetch-Mode: cors\" -H \"Sec-Fetch-Dest: empty\" -H \"Referer: https://zakupki.sportmaster.ru/tender_list.php?page=1\" -H \"Accept-Language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7\" -H \"Cookie: BITRIX_SM_SALE_UID=0; BITRIX_SM_LOGIN=info_enter-it_ru; BITRIX_SM_SOUND_LOGIN_PLAYED=Y; PHPSESSID=HubvYEBLfpg6ncELx0o4Tif0qNfDvOYr; BITRIX_SM_UIDH=1de806127e9274a8c5b8b4e07201d6a8; BITRIX_SM_UIDL=info_enter-it_ru\" --data-raw \"order[id]=desc&offset={page}&creatorId=0&&SITE_ID=s1&sessid=bcc39fb34249a51335b3ddb5d61f68b5&SITE_TEMPLATE_ID=sportmaster\"";
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