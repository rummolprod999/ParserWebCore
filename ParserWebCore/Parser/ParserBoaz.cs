using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.Creators;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserBoaz : ParserAbstract, IParser
    {
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);
        private const string Url = "http://boaz-konkurs.ru/Actuals.aspx";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private List<TypeBoaz> _tendersList = new List<TypeBoaz>();

        public void Parsing()
        {
            Parse(ParsingBoaz);
        }

        private void ParsingBoaz()
        {
            try
            {
                ParserSelenium();
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
            foreach (var tt in _tendersList)
            {
                var tn = new TenderBoaz("АО «Организатор строительства Богучанского Алюминиевого Завода»",
                    "http://boaz-konkurs.ru/", 257,
                    tt);
                ParserTender(tn);
            }
        }

        private void ParserSelenium()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(Url);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//table/tbody/tr[contains(@class, 'ms-itmhover')]")));
            ParsingList();
        }

        private void ParsingList()
        {
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//table/tbody/tr[contains(@class, 'ms-itmhover')]"));
            foreach (var t in tenders)
            {
                try
                {
                    ParsingPage(t);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(IWebElement t)
        {
            var purName =
                t.FindElementWithoutException(By.XPath(".//div/span"))?.Text
                    .Trim() ?? "";
            var href = t.FindElementWithoutException(By.XPath(".//div[@onclick]"))?.GetAttribute("onclick")
                           .Trim() ??
                       throw new Exception("cannot find href");
            href = href.GetDataFromRegex(@"location='(http.+)'$");
            var purNum = href.GetDataFromRegex(@"ID=(\d+)");
            if (purNum == "")
            {
                throw new Exception("cannot find purNum");
            }

            var datePubT =
                t.FindElementWithoutException(By.XPath(".//td[3]/nobr"))?.Text
                    .Trim() ??
                throw new Exception("cannot find datePubT");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEndTt =
                t.FindElementWithoutException(By.XPath(".//td[4]/nobr"))
                    ?.Text.Trim() ??
                throw new Exception("cannot find dateEndT");
            var dateEnd = dateEndTt.ParseDateUn("dd.MM.yyyy HH:mm");
            var tt = new TypeBoaz
            {
                PurName = purName,
                PurNum = purNum,
                DatePub = datePub,
                DateEnd = dateEnd,
                Href = href
            };
            _tendersList.Add(tt);
        }
    }
}