using System;
using HtmlAgilityPack;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSochipark : ParserAbstract, IParser
    {
        private const string Urlpage =
            "https://www.sochipark.ru/cooperation/tenders/kvalifikatsionnyy-otbor-7?PAGEN_1=";

        public void Parsing()
        {
            Parse(ParsingSochi);
        }

        private void ParsingSochi()
        {
            for (var i = 1; i <= 8; i++)
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
                $"\"https://www.sochipark.ru/cooperation/tenders/?PAGEN_1={page}\" -H \"Connection: keep-alive\" -H \"Cache-Control: max-age=0\" -H \"sec-ch-ua: \" Not;A Brand\";v=\"99\", \"Google Chrome\";v=\"91\", \"Chromium\";v=\"91\"\" -H \"sec-ch-ua-mobile: ?0\" -H \"Upgrade-Insecure-Requests: 1\" -H \"User-Agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36\" -H \"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9\" -H \"Sec-Fetch-Site: none\" -H \"Sec-Fetch-Mode: navigate\" -H \"Sec-Fetch-User: ?1\" -H \"Sec-Fetch-Dest: document\" -H \"Accept-Language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7\"";
            var docString =
                CurlDownloadSportMaster.DownL(arguments);
            if (string.IsNullOrEmpty(docString))
            {
                Log.Logger("Empty string in ParserPage()", page);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(docString);
            var tens =
                htmlDoc.DocumentNode.SelectNodes(
                    "//div[@class = 'grid-row list']/div") ??
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
            var href = (n.SelectSingleNode(".//a")?.Attributes["href"]?.Value ?? "")
                .Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            href = $"https://www.sochipark.ru{href}";
            var purName = (n.SelectSingleNode(".//div[@class = 'desc']/p[1]")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            var purNum = (n.SelectSingleNode(".//div[@class = 'title']")
                ?.InnerText ?? "").ReplaceHtmlEntyty().DelDoubleWhitespace().Trim();
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum", href);
                return;
            }

            var nmck = (n.SelectSingleNode(".//p[contains(., 'Цена тендера:')]")
                ?.InnerText ?? "").Trim().ReplaceHtmlEntyty();
            nmck = nmck.ExtractPriceNew();
            var datePub = DateTime.Today;
            var dateEndT =
                (n.SelectSingleNode(".//p[contains(., 'Дата завершения:')]")
                    ?.InnerText ?? "").Trim();
            dateEndT = dateEndT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var tn = new TenderSochipark("Сочи Парк", "https://www.sochipark.ru/", 324,
                new TypeSochipark
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Nmck = nmck
                });
            ParserTender(tn);
        }
    }
}