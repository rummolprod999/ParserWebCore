#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.Creators;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserSegezhaNew : ParserAbstract, IParser
    {
        private const int Count = 50;
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();

        private readonly string[] Url =
        {
            "https://segezha-group.com/providers/purchasing/?status=1",
            "https://segezha-group.com/providers/qualification/"
        };

        private readonly List<TypeSegezha> _tendersList = new List<TypeSegezha>();
        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(60);

        public void Parsing()
        {
            Parse(ParsingSegezha);
        }

        private void ParsingSegezha()
        {
            try
            {
                foreach (var s in Url)
                {
                    ParserSelenium(s);
                }

                ParserListTenders();
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }
            finally
            {
                _driver.Manage().Cookies.DeleteAllCookies();
                _driver.Quit();
            }
        }

        private void ParserListTenders()
        {
            foreach (var tn in _tendersList.Select(tt =>
                         new TenderSegezha("ГК Сегежа", "https://segezha-group.com", 97, tt)))
            {
                ParserTender(tn);
            }
        }

        private void ParserSelenium(string Url)
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(Url);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//tbody[@class = 'js-more-purchase']/tr")));
            ParsingList(wait, Url);
        }

        private void ParsingList(WebDriverWait wait, string Url)
        {
            _driver.SwitchTo().DefaultContent();
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    clicker(wait);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }

            Thread.Sleep(2000);
            _driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//tbody[@class = 'js-more-purchase']/tr"));
            foreach (var t in tenders)
            {
                try
                {
                    ParsingPage(t, Url);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void clicker(WebDriverWait wait)
        {
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//button[contains(., 'Показать еще')]")));
            Thread.Sleep(2000);
            _driver.SwitchTo().DefaultContent();
            _driver.ExecutorJs(
                "var elem = document.querySelectorAll('button[x-show=\"nextPage !== false\"]'); elem[0].click()");
            //_driver.FindElement(By.XPath("//button[contains(., 'Показать еще')]")).Click();
        }

        private void ParsingPage(IWebElement t, string url)
        {
            var href = t.FindElementWithoutException(By.XPath("."))?.GetAttribute("@click")
                           .Trim() ??
                       throw new Exception("cannot find @click");
            var hrefT = href.GetDataFromRegex(@"location\s+=\s+'(\d+)/'$");
            href = $"{url}{hrefT}/";
            href = href.Replace("?status=1", "");
            var purNum = hrefT;
            var purName = t.FindElementWithoutException(By.XPath(".//td[3]"))?.Text
                .Trim() ?? "";
            var status = t.FindElementWithoutException(By.XPath(".//td[6]"))?.Text
                .Trim() ?? "";
            var cusName = t.FindElementWithoutException(By.XPath(".//td[4]"))?.Text
                .Trim() ?? "";
            var datePubT =
                t.FindElementWithoutException(By.XPath(".//td[1]"))?.Text
                    .Trim() ??
                throw new Exception("cannot find datePubT");
            var datePub = datePubT.ParseDateUn("dd.MM.yy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndTt =
                t.FindElementWithoutException(By.XPath(".//td[2]"))
                    ?.Text.Trim() ??
                throw new Exception("cannot find dateEndT");
            var dateEnd = dateEndTt.DelDoubleWhitespace().ParseDateUn("dd.MM.yy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var tt = new TypeSegezha
            {
                CusName = cusName, DateEnd = dateEnd, DatePub = datePub, Href = href, OrgName = cusName,
                PurName = purName, PurNum = purNum, Status = status
            };
            _tendersList.Add(tt);
        }
    }
}