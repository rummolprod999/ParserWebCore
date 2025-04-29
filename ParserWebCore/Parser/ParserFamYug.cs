#region

using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using ParserWebCore.Creators;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserFamYug : ParserAbstract, IParser
    {
        private const int Count = 5;
        private const string Url = "https://etp.family-yug.ru/index.php?r=trades%2Findex";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(60);
        private readonly List<TypeFamYug> _urls = new List<TypeFamYug>();

        public void Parsing()
        {
            Parse(ParsingFamYug);
        }

        private void ParsingFamYug()
        {
            try
            {
                ParserSelenium();
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

        private void ParserSelenium()
        {
            Auth();
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(Url);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//table//tr[@data-key]")));
            ParsingList();
            for (var i = 0; i < Count; i++)
            {
                _driver.SwitchTo().DefaultContent();
                try
                {
                    wait.Until(dr =>
                        dr.FindElement(By.XPath("//li[@class='next']/a")));
                    _driver.SwitchTo().DefaultContent();
                }
                catch (Exception)
                {
                    Log.Logger("This is last page, return");
                    return;
                }

                try
                {
                    _driver.ExecutorJs(
                        "var elem = document.querySelectorAll('li.next a'); elem[0].click()");
                    Thread.Sleep(5000);
                }
                catch (Exception)
                {
                    Log.Logger("This is last page, return");
                }

                wait.Until(dr =>
                    dr.FindElement(By.XPath(
                        "//table//tr[@data-key]")));
                _driver.SwitchTo().DefaultContent();
                ParsingList();
            }

            _urls.ForEach(x =>
            {
                try
                {
                    var tn = new TenderFamYug("СК Семья", "https://etp.family-yug.ru/", 395,
                        x
                        , _driver);
                    ParserTender(tn);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            });
        }

        private void Auth()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl("https://etp.family-yug.ru/index.php?r=user%2Flogin");
            Thread.Sleep(5000);
            _driver.SwitchTo().DefaultContent();
            _driver.FindElement(By.XPath("//input[@id = 'loginform-username']")).SendKeys(AppBuilder.FamYugUser);
            _driver.FindElement(By.XPath("//input[@id = 'loginform-password']")).SendKeys(AppBuilder.FamYugPass);
            _driver.FindElement(By.XPath("//button[@type = 'submit']")).Click();
            Thread.Sleep(5000);
        }

        private void ParsingList()
        {
            _driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//table//tr[@data-key]"));
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
            var purNum = t.FindElementWithoutException(By.XPath("./td[1]"))?.Text.Trim() ??
                         throw new Exception("cannot find purNum");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var purName = t.FindElementWithoutException(By.XPath("./td[2]"))?.Text.Trim() ??
                          throw new Exception("cannot find purName");
            var pwName = t.FindElementWithoutException(By.XPath("./td[3]"))?.Text.Trim() ??
                         "";
            var datePubT =
                t.FindElementWithoutException(By.XPath("./td[5]"))?.Text
                    .Trim() ??
                "";
            var datePub = datePubT.ParseDateUn("yyyy-MM-dd HH:mm:ss");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndT =
                t.FindElementWithoutException(By.XPath("./td[6]"))
                    ?.Text.Trim() ??
                "";
            var dateEnd = dateEndT.ParseDateUn("yyyy-MM-dd HH:mm:ss");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd");
                return;
            }

            var status = t.FindElementWithoutException(By.XPath("./td[7]"))?.Text.Trim() ??
                         "";
            var tp = new TypeFamYug
            {
                PlacingWay = pwName,
                DateEnd = dateEnd,
                DatePub = datePub,
                Href = "https://etp.family-yug.ru/index.php?r=trades%2Findex",
                PurNum = purNum,
                PurName = purName,
                Status = status
            };
            _urls.Add(tp);
        }
    }
}