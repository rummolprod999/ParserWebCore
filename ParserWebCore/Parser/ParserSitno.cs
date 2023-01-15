using System;
using Newtonsoft.Json.Linq;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSitno : ParserAbstract, IParser
    {
        private const int Count = 5;

        public void Parsing()
        {
            Parse(ParsingSitno);
        }

        private void ParsingSitno()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage =
                    $"https://tender.sitno.ru:9000/api/trade-offer/showcase?page={i}&expand=status,company,file,user,bidder,is_follow,categories,procedureType&search[Active]=active&per-page=9&sort=-published_at";
                try
                {
                    ParsingPage(urlpage);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(string url)
        {
            var s = DownloadString.DownL(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var jObj = JObject.Parse(s);
            var tenders = GetElements(jObj, "data");
            foreach (var a in tenders)
            {
                try
                {
                    ParserTender(a);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTender(JToken t)
        {
            var href = "https://tender.sitno.ru/showcase";

            var purNum = ((string)t.SelectToken("id") ?? "").Trim();
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var purName = ((string)t.SelectToken("name") ?? "").Trim();
            var orgName = "Компания СИТНО";
            var contactPerson = ((string)t.SelectToken("user.name") ?? "").Trim();
            var phone = ((string)t.SelectToken("bidder[0].phone") ?? "").Trim();
            var datePubT =
                ((string)t.SelectToken("published_at") ?? "").Trim();
            var datePub = datePubT.ParseDateUn("yyyy-MM-dd HH:mm");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                ((string)t.SelectToken("duration") ?? "").Trim();
            var dateEnd = dateEndT.ParseDateUn("yyyy-MM-dd HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var tn = new TenderAgrokomplex("Компания СИТНО", "http://sitno.ru/tender/", 103,
                new TypeAgrokomplex
                {
                    OrgName = orgName,
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    ContactPerson = contactPerson,
                    Phone = phone
                });
            ParserTender(tn);
        }
    }
}