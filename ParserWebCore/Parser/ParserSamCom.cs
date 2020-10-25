using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSamCom : ParserAbstract, IParser
    {
        private readonly string _urlpage = "https://samcomsys.ru/purchaselist?purchasestatus_id=13";

        public void Parsing()
        {
            Parse(ParsingSamCom);
        }

        private void ParsingSamCom()
        {
            try
            {
                ParsingPage(_urlpage);
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
                    "//div[@class = 'purchase-second-line']") ??
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
            var href = _urlpage;
            var purName =
                n.SelectSingleNode(".//span[. = 'Наименование:']/following-sibling::span")?.InnerText?.Trim() ??
                throw new Exception(
                    $"cannot find purName in {href}");
            purName = HttpUtility.HtmlDecode(purName);
            var purNum =
                n.SelectSingleNode(".//span[. = 'Номер заказа:']/following-sibling::span")?.InnerText?.Trim() ??
                throw new Exception(
                    $"cannot find purNum in {href}");
            var nmck =
                n.SelectSingleNode(".//span[. = 'Начальная максимальная стоимость закупки:']/following-sibling::span")
                    ?.InnerText?.Trim() ?? throw new Exception(
                    $"cannot find nmck in {href}");
            nmck = nmck.GetDataFromRegex(@"([\d.]+)");
            var datePubT =
                n.SelectSingleNode(".//span[. = 'Дата публикации:']/following-sibling::span")?.InnerText?.Trim() ??
                throw new Exception(
                    $"cannot find datePubT in {href}");
            datePubT = HttpUtility.HtmlDecode(datePubT).DelDoubleWhitespace();
            var myCultureInfo = new CultureInfo("ru-RU");
            var datePub = DateTime.Parse(datePubT, myCultureInfo);
            var dateEndT =
                n.SelectSingleNode(".//span[. = 'Дата окончания приема заявок:']/following-sibling::span")?.InnerText
                    ?.Trim() ?? throw new Exception(
                    $"cannot find dateEndT in {href}");
            dateEndT = HttpUtility.HtmlDecode(dateEndT).DelDoubleWhitespace();
            var dateEnd = DateTime.Parse(dateEndT, myCultureInfo);
            var attachments = new List<TypeSamCom.Attachment>();
            var atts = n.SelectNodes(@".//a[@class = 'download-document']");
            foreach (var a in atts)
            {
                var name = a.InnerText.Trim();
                var url = a.GetAttributeValue("href", "").Trim();
                if (name == "" || url == "") continue;
                url = $"https://samcomsys.ru{url}";
                attachments.Add(new TypeSamCom.Attachment {Name = name, Url = url});
            }

            var tn = new TenderSamCom("ООО «РКС-Холдинг»", "https://samcomsys.ru/", 251,
                new TypeSamCom
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                    Attachments = attachments, Nmck = nmck
                });
            ParserTender(tn);
        }
    }
}