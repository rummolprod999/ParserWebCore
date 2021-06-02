using System;
using System.Threading;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSochiPark2 : ParserAbstract, IParser
    {
        public void Parsing()
        {
            Parse(ParsingSochi);
        }

        private void ParsingSochi()
        {
            try
            {
                ParsingPage("https://zakup.sochipark.ru/purchases/");
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }

            for (var i = 1; i <= 11; i++)
            {
                try
                {
                    var url = $"https://zakup.sochipark.ru/purchases/?PAGEN_1={i}";
                    ParsingPage(url);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(string page)
        {
            var arguments =
                $"\"{page}\"";
            var docString =
                CurlDownloadSportMaster.DownL(arguments);
            if (string.IsNullOrEmpty(docString))
            {
                Log.Logger("Empty string in ParserPage()", page);
                return;
            }

            var s = DownloadString.DownL1251(page);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", page);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(docString);
            var tens =
                htmlDoc.DocumentNode.SelectNodes("//div[@class = 'news-detail']") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserTender(a);
                    Thread.Sleep(5000 * 4);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTender(HtmlNode n)
        {
            var href = (n.SelectSingleNode(".//div[@class = 'lot-header-inner']/a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://zakup.sochipark.ru{href}";
            var purName = (n.SelectSingleNode(".//div[@class = 'lot-header-inner']/a")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = purName.GetDataFromRegex(@"\s+([\d\-5–]+)\s+");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", purName);
                return;
            }

            var nmckT = (n.SelectSingleNode(".//label[contains(., 'Начальная цена:')]/following-sibling::span")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var nmck = nmckT.ExtractPriceNew();
            var currency = nmckT.GetDataFromRegex(@"\|(.+)");
            var datePub = DateTime.Today;
            var dateEndT =
                (n.SelectSingleNode(".//label[contains(., 'Дата окончания подачи заявок:')]/following-sibling::span")
                    ?.InnerText ?? "").Trim();
            //datePubT = datePubT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var pwName = (n.SelectSingleNode(".//label[contains(., 'Способ закупки:')]/following-sibling::span")
                ?.InnerText ?? "").ReplaceHtmlEntyty().DelDoubleWhitespace().Trim();
            var status = (n.SelectSingleNode(".//li[contains(@class, 'lot-status')]")
                ?.InnerText ?? "").ReplaceHtmlEntyty().DelDoubleWhitespace().Trim();
            var tn = new TenderSochipark2("Сочи Парк", "https://www.sochipark.ru/", 324,
                new TypeSochipark
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Nmck = nmck,
                    PwName = pwName,
                    Status = status,
                    Currency = currency
                });
            ParserTender(tn);
        }
    }
}