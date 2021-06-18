using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserEurosib : ParserAbstract, IParser
    {
        private const string Urlpage = "https://www.eurosib-td.ru/ru/zakupki-rabot-i-uslug/";

        public void Parsing()
        {
            Parse(ParsingEurosib);
        }

        private void ParsingEurosib()
        {
            try
            {
                ParsingPage(Urlpage);
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
                    "//table[@id = 'd']//tr[contains(., 'Дата начала подачи заявок:')]") ??
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
            var purName = "";
            var purNum =
                n.SelectSingleNode(".//a")?.InnerText?.Replace("№", "")
                    .Trim().ReplaceHtmlEntyty() ?? throw new Exception(
                    $"cannot find purNum");
            var href =
                n.SelectSingleNode(".//a")?.Attributes["href"]?.Value ??
                throw new Exception(
                    $"Cannot find href in {purNum}");
            href = $"https://www.eurosib-td.ru{href}";
            var cusName = "";
            var pubDateT =
                n.SelectSingleNode(".//p[contains(., 'Дата начала подачи заявок:')]")?.InnerText
                    ?.Replace("Дата начала подачи заявок:", "")
                    .Trim().ReplaceHtmlEntyty().DelDoubleWhitespace() ?? throw new Exception(
                    $"cannot find pubDateT");
            var datePub = pubDateT.ParseDateUn("dd/MM/yyyy HH:mm");
            if (datePub == DateTime.MinValue)
            {
                datePub = pubDateT.ParseDateUn("dd/MM/yyyy");
            }

            if (datePub == DateTime.MinValue)
            {
                datePub = DateTime.Today;
            }

            var endDateT =
                n.SelectSingleNode(".//p[contains(., 'Дата окончания приема заявок:')]")?.InnerText
                    ?.Replace("Дата окончания приема заявок:", "")
                    .Trim().ReplaceHtmlEntyty().DelDoubleWhitespace() ?? "";
            var dateEnd = endDateT.ParseDateUn("dd/MM/yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = endDateT.ParseDateUn("dd/MM/yyyy");
            }

            if (dateEnd == DateTime.MinValue)
            {
                datePub.AddDays(2);
            }

            var tn = new TenderEurosib("ООО «Торговый дом «ЕвроСибЭнерго»", "https://www.eurosib-td.ru/", 329,
                new TypeEurosib
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                    CusName = cusName
                });
            ParserTender(tn);
        }
    }
}