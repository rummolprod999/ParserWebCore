using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSegz : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingMetPort);
        }

        private void ParsingMetPort()
        {
            try
            {
                ParsingPage($"https://segz.ru/tenders/");
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
                htmlDoc.DocumentNode.SelectNodes("//a[contains(@class, 'tender__card')]") ??
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
            var purName = (n.SelectSingleNode(".//p[@class = 'tender__title']")
                ?.InnerText ?? "").Trim().DelDoubleWhitespace();
            if (string.IsNullOrEmpty(purName))
            {
                Log.Logger("Empty purName");
                return;
            }

            var purNumT = (n.SelectSingleNode(".//span[@class = 'tender__number']")
                ?.InnerText ?? "").Trim();
            var purNum = purNumT.GetDataFromRegex("(\\d+)");
            if (string.IsNullOrEmpty(purNum))
            {
                purNum = purName.ToMd5();
            }

            var href =
                n.Attributes["href"]?.Value ??
                throw new Exception(
                    $"Cannot find href in {purNum}");
            href = $"https://segz.ru{href}";
            var pwName = (n.SelectSingleNode(".//p[@class = 'tender__purchase']")
                ?.InnerText ?? "").Trim().DelDoubleWhitespace().Replace(purNumT, "").Replace("-", "").Trim();
            var dates =
                n.SelectSingleNode(".//div[@class = 'tender__info']/p[2]")?.InnerText?.Trim().Replace("Период:", "")
                    .DelDoubleWhitespace() ??
                throw new Exception(
                    $"Cannot find dates in {href}");
            var datePubT = dates.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})\s+");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEndT = dates.GetDataFromRegex(@"-\s+(\d{2}\.\d{2}\.\d{4})");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue || dateEnd == DateTime.MinValue)
            {
                throw new Exception(
                    $"Cannot find date pub or date end in {href}");
            }

            var tn = new TenderSegz("Сарапульский электрогенераторный завод", "https://segz.ru/", 401,
                new TypeSegz
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    PwName = pwName
                });
            ParserTender(tn);
        }
    }
}