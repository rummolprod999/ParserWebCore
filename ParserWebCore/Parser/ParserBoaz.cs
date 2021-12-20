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
        private const string Url = "https://boaz-zavod.ru/suppliers/tenders/";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private List<TypeBoaz> _tendersList = new List<TypeBoaz>();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);

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
                    "//table[@itemscope]/tbody/tr")));
            ParsingList();
        }

        private void ParsingList()
        {
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//table[@itemscope]/tbody/tr"));
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
                t.FindElementWithoutException(By.XPath(".//a"))?.Text
                    .Trim() ?? "";
            var href = t.FindElementWithoutException(By.XPath(".//a"))?.GetAttribute("href")
                           .Trim() ??
                       throw new Exception("cannot find href");
            var purNum = href.ToMd5();

            var datePubT =
                t.FindElementWithoutException(By.XPath(".//td[1]"))?.Text
                    .Trim() ??
                throw new Exception("cannot find datePubT");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            var dateEndTt =
                t.FindElementWithoutException(By.XPath(".//td[3]"))
                    ?.Text.Trim() ??
                throw new Exception("cannot find dateEndT");
            var dateEnd = dateEndTt.ParseDateUn("dd.MM.yyyy");
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