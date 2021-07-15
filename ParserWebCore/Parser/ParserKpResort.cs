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
    public class ParserKpResort : ParserAbstract, IParser
    {
        private readonly string _urlpage = "https://zakup.kpresort.ru/purchases/?PAGEN_1=";
        public int UPPER => 30;

        public void Parsing()
        {
            Parse(ParsingKpResort);
        }

        private void ParsingKpResort()
        {
            try
            {
                ParsingPage("https://zakup.kpresort.ru/purchases/?SHOWALL_1=1");
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }

            for (int i = 0; i <= UPPER; i++)
            {
                try
                {
                    Thread.Sleep(5000);
                    ParsingPage($"{_urlpage}{i}");
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(string url)
        {
            var s = DownloadString.DownL(url, tryCount: 5);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//div[@class = 'news-detail']") ??
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
            var href = (n.SelectSingleNode(".//a[contains(., 'Перейти в Лот')]")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://zakup.kpresort.ru{href}";
            var purName = n.SelectSingleNode(".//div[@class = 'lot-header-inner']/a")?.InnerText?.Trim() ??
                          throw new Exception(
                              $"cannot find purName in {href}");
            var purNum = purName.GetDataFromRegex(@"ЛОТ\s*([\d\s-–]+)\s+").DelAllWhitespace();
            if (string.IsNullOrEmpty(purNum))
            {
                purNum = href.GetDataFromRegex(@"purchases/(\d+)/");
            }

            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var status =
                n.SelectSingleNode(".//li[contains(@class, 'lot-status')]")?.InnerText?.Trim() ?? "";
            var pwName =
                n.SelectSingleNode(".//label[contains(., 'Способ закупки:')]/following-sibling::span")?.InnerText
                    ?.Trim() ?? "";
            var nmck =
                n.SelectSingleNode(".//label[contains(., 'Начальная цена:')]/following-sibling::span")?.InnerText
                    ?.Trim().ExtractPriceNew() ?? "";
            var currency =
                n.SelectSingleNode(".//label[contains(., 'Начальная цена:')]/following-sibling::span")?.InnerText
                    ?.Trim().GetDataFromRegex(@"\|(\w+)").Trim() ?? "";
            var delivTerm =
                n.SelectSingleNode(".//div[contains(@class, 'lot-text')]")?.InnerText?.ReplaceHtmlEntyty().Trim() ?? "";
            var datePub = DateTime.Today;
            var dateEndT =
                n.SelectSingleNode(".//label[contains(., 'Дата окончания подачи заявок:')]/following-sibling::span")
                    ?.InnerText
                    ?.Trim().DelDoubleWhitespace() ?? throw new Exception(
                    $"cannot find dateEndT in {href}");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm:ss");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var tn = new TenderKpResort("НАО «Красная поляна»", "https://zakup.kpresort.ru/", 341,
                new TypeKpResort
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                    PwName = pwName, Nmck = nmck, DelivTerm = delivTerm.DelDoubleWhitespace(), Currency = currency,
                    Status = status
                });
            ParserTender(tn);
        }
    }
}