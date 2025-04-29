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
    public class ParserMetal100 : ParserAbstract, IParser
    {
        private const int Count = 2;
        private readonly ChromeDriver _driver = CreateChomeDriverNoHeadless.GetChromeDriver();

        private readonly string[] Url =
        {
            "https://metal100.ru/tenders"
        };

        private readonly List<TypeMetal100> _tendersList = new List<TypeMetal100>();
        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(60);

        public void Parsing()
        {
            Parse(ParsingMetal100);
        }

        private void ParsingMetal100()
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
                         new TenderMetal100("METAL100.RU", "https://metal100.ru/", 390, tt, _driver)))
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
                    "//div[contains(@class, 'tenderRow')]")));
            ParsingCurrentPage(Url);
            ParsingList(wait, Url);
        }

        private void ParsingList(WebDriverWait wait, string Url)
        {
            _driver.SwitchTo().DefaultContent();
            for (var i = 0; i < 20; i++)
            {
                try
                {
                    clicker(wait);
                    ParsingCurrentPage(Url);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingCurrentPage(string Url)
        {
            Thread.Sleep(2000);
            _driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//div[contains(@class, 'tenderRow')]"));
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
                    "//a[@title = 'Следующая']")));
            Thread.Sleep(2000);
            _driver.SwitchTo().DefaultContent();
            _driver.ExecutorJs(
                "var elem = document.querySelectorAll('a[title = \"Следующая\"]'); elem[0].click()");
            //_driver.FindElement(By.XPath("//button[contains(., 'Показать еще')]")).Click();
        }

        private void ParsingPage(IWebElement t, string url)
        {
            var href = t.FindElementWithoutException(By.XPath(".//a"))?.GetAttribute("href")
                           .Trim() ??
                       throw new Exception("cannot find href");
            var purName = t.FindElementWithoutException(By.XPath(".//tbody/tr[1]/td[2]/strong"))?.Text
                .Trim() ?? "";
            var purNum = t.FindElementWithoutException(By.XPath(".//a/strong"))?.Text?.Replace("Тендер №", "")
                .Trim() ?? throw new Exception("cannot find purNum");
            var pwName = t.FindElementWithoutException(By.XPath(".//em/following-sibling::span"))?.Text
                .Trim() ?? "";
            var datePubT =
                t.FindElementWithoutException(By.XPath(".//span/preceding-sibling::em/strong[1]"))?.Text
                    .Trim() ??
                throw new Exception("cannot find datePubT");
            datePubT = datePubT.GetDateWithMonthFull().Replace(" г.", "");
            Console.WriteLine("DP " + datePubT);
            var datePub = datePubT.DelDoubleWhitespace().ParseDateUn("H:mm dd MM yyyy");

            var dateEndT =
                t.FindElementWithoutException(By.XPath(".//span/preceding-sibling::em/strong[2]"))?.Text
                    .Trim() ??
                throw new Exception("cannot find dateEndT");
            dateEndT = dateEndT.GetDateWithMonthFull().Replace(" г.", "");
            Console.WriteLine("DE " + dateEndT);
            var dateEnd = dateEndT.DelDoubleWhitespace().ParseDateUn("H:mm d MM yyyy");
            var status = t.FindElementWithoutException(By.XPath(".//tbody/tr[1]/td[5]"))?.Text
                .Trim() ?? "";
            var tt = new TypeMetal100
            {
                DateEnd = dateEnd, DatePub = datePub, Href = href,
                PurName = purName, PurNum = purNum, Status = status, PwName = pwName
            };
            _tendersList.Add(tt);
        }
    }
}