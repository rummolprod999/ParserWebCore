using System;
using OpenQA.Selenium;
using ParserWebCore.BuilderApp;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserMidural : ParserGzwSp
    {
        public ParserMidural(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg,
            int count = 10) : base(url, baseurl, etpName, etpUrl, typeFz, arg, count)
        {
        }

        protected override void ParsingPage(IWebElement t)
        {
            var purName =
                t.FindElementWithoutException(By.XPath(".//span[@class = 'name']"))?.Text
                    .Trim() ?? "";
            if (string.IsNullOrEmpty(purName))
            {
                purName =
                    t.FindElementWithoutException(
                            By.XPath(".//span[. = 'Объект исследования']/following-sibling::span"))?.Text
                        .Trim() ??
                    throw new Exception("cannot find purName ");
            }

            var href = t.FindElementWithoutException(By.XPath(".//span[@class = 'name']/a"))?.GetAttribute("href")
                           .Trim() ??
                       throw new Exception("cannot find href");
            var purNum =
                t.FindElementWithoutException(By.XPath(".//span[contains(@class, 'regnumber')]"))?.Text.Trim() ??
                throw new Exception("cannot find purNum ");
            var datePubT =
                t.FindElementWithoutException(By.XPath(".//span[. = 'Дата публикации']/following-sibling::span"))?.Text
                    .Trim() ??
                throw new Exception("cannot find datePubT");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm");
            }

            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndTt =
                t.FindElementWithoutException(
                        By.XPath(".//span[. = 'Предложения принимаются до']/following-sibling::span"))
                    ?.Text.Trim() ??
                throw new Exception("cannot find dateEndT");
            var dateEndT = dateEndTt.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4}\s*\d{2}:\d{2})$").DelDoubleWhitespace();
            if (string.IsNullOrEmpty(dateEndT))
            {
                dateEndT = dateEndTt.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})$").DelDoubleWhitespace();
            }

            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            }

            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
            }

            var status =
                t.FindElementWithoutException(By.XPath(".//td/span[contains(@class, 'status')]"))?.Text.Trim() ??
                "";
            var cusName = t.FindElementWithoutException(By.XPath(".//span[@class = 'customer']/a"))?.Text.Trim() ?? "";
            var cusInn = t.FindElementWithoutException(By.XPath(".//span[. = 'ИНН']/following-sibling::span"))
                ?.Text.Replace("ИНН", "").Trim() ?? "";
            var nmck = t.FindElementWithoutException(By.XPath(".//td/span[contains(@class, 'nmck')]"))?.Text
                           .DelAllWhitespace().Trim() ??
                       "";
            var tt = new TypeMzVoron
            {
                PurName = purName,
                PurNum = purNum,
                CusInn = cusInn,
                CusName = cusName,
                DatePub = datePub,
                DateEnd = dateEnd,
                Nmck = nmck,
                Status = status,
                Href = href
            };
            _tendersList.Add(tt);
        }
    }
}