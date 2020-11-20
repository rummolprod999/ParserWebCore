using System;
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
    public class ParserAgat : ParserAbstract, IParser
    {
        private const int Count = 5;

        public void Parsing()
        {
            Parse(ParsingAgat);
        }

        private void ParsingAgat()
        {
            for (var i = 1; i <= Count; i++)
            {
                var urlpage = $"https://agat-group.com/about/purchasing/?page={i}";
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
            var s = DownloadString.DownLUserAgent(url);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes("//div[@class = 'purchasing__list--item']") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserTender(a, url);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTender(HtmlNode n, string url)
        {
            var href = url;
            var purName = n.SelectSingleNode(".//div[@class = 'purchasing__list---type']")?.InnerText?.Trim() ??
                          throw new Exception(
                              $"cannot find purName in {href}");
            var purNum = purName.ToMd5();
            var status = n.SelectSingleNode(".//div[contains(@class, 'purchasing__list--state')]")?.InnerText?.Trim() ??
                         throw new Exception(
                             $"cannot find status in {href}");
            var dates = n.SelectSingleNode(".//div[. = 'Подача заявок']/following-sibling::div")?.InnerText?.Trim() ??
                        throw new Exception(
                            $"cannot find dates in {href}");
            var year = n.SelectSingleNode(".//div[. = 'Подача заявок']/following-sibling::div[last()]")?.InnerText
                ?.Trim() ?? throw new Exception(
                $"cannot find year in {href}");
            var yearStart = year.GetDataFromRegex(@"^(\d{4})");
            var yearEnd = year.GetDataFromRegex(@"(\d{4})$");
            var startDateT = dates.GetDataFromRegex(@"(.+)\s-");
            var endDateT = dates.GetDataFromRegex(@"-\s(.+)");
            var myCultureInfo = new CultureInfo("ru-RU");
            var datePub = DateTime.Parse($"{startDateT} {yearStart}", myCultureInfo);
            var dateEnd = DateTime.Parse($"{endDateT} {yearEnd}", myCultureInfo);
            var conditions = n.SelectNodes(".//div[@class = 'purchasing__list--desc']");
            var requirements = "";
            foreach (var htmlNode in conditions)
            {
                requirements += $" {htmlNode.InnerText}\n";
            }

            requirements = HttpUtility.HtmlDecode(requirements).Trim();
            var tn = new TenderAgat("ООО «ТД «Агат»", "https://agat-group.com", 241,
                new TypeAgat
                {
                    DateEnd = dateEnd,
                    DatePub = datePub,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Status = status,
                    Requirements = requirements
                });
            ParserTender(tn);
        }
    }
}