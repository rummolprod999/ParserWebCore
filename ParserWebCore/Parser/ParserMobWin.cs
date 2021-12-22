using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserMobWin : ParserAbstract, IParser
    {
        private const string _url = "https://www.mobile-win.ru/about/procurements";

        public void Parsing()
        {
            Parse(ParsingMobWin);
        }

        private void ParsingMobWin()
        {
            try
            {
                ParsingPage(_url);
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }
        }

        private void ParsingPage(string url)
        {
            var s = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//div[@id = 'activeProcurements']/div[contains(@class, 'js-spoilerId')]") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
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

        private void ParserTender(HtmlNode n)
        {
            var purName = n.SelectSingleNode(".//h3")?.InnerText?.Trim() ?? throw new Exception(
                $"cannot find purName in {_url}");
            var text = n.InnerText.DelDoubleWhitespace();
            var datePubT = text.GetDataFromRegex(@"(?:Дата\s+начала|Начало).+?(\d{2}\.\d{2}\.\d{4})");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", purName);
                return;
            }

            var dateEndT = text.GetLastDataFromRegex(@"(?:Дата\s+окончания|Окончание).+?(\d{2}\.\d{2}\.\d{4})");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var purNum = $"{purName}{datePub:s}".ToMd5();
            var attachments = new List<TypeSamCom.Attachment>();
            var atts = n.SelectNodes(@".//a");
            foreach (var a in atts)
            {
                var name = a.InnerText.Trim();
                var url = a.GetAttributeValue("href", "").Trim();
                if (name == "" || url == "") continue;
                attachments.Add(new TypeSamCom.Attachment { Name = name, Url = url });
            }

            var notice = text.GetDataFromRegex(@"(Контактное\s+лицо.+)");
            var tn = new TenderMobWin("ООО \"К-телеком\"", "https://www.mobile-win.ru", 360,
                new TypeMobWin
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = _url,
                    PurNum = purNum,
                    PurName = purName,
                    Notice = notice,
                    Attachments = attachments,
                });
            ParserTender(tn);
        }
    }
}