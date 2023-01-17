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

namespace ParserWebCore.Parser
{
    public class ParserAgrokomplexNew : ParserAbstract, IParser
    {
        private const int Count = 5;
        private const string Url = "https://atp.agrokomplex.ru/lots";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);
        private List<TypeAgrokomplex> _urls = new List<TypeAgrokomplex>();

        public void Parsing()
        {
            Parse(ParsingAgro);
        }

        private void ParsingAgro()
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
                    "//lot")));
            ParsingList();
            for (var i = 0; i < Count; i++)
            {
                _driver.SwitchTo().DefaultContent();
                try
                {
                    wait.Until(dr =>
                        dr.FindElement(By.XPath("//button[contains(@class, 'mat-paginator-navigation-next')]")));
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
                        "var elem = document.querySelectorAll('button.mat-paginator-navigation-next'); elem[0].click()");
                    Thread.Sleep(5000);
                }
                catch (Exception)
                {
                    Log.Logger("This is last page, return");
                }

                wait.Until(dr =>
                    dr.FindElement(By.XPath(
                        "//lot")));
                _driver.SwitchTo().DefaultContent();
                ParsingList();
            }

            _urls.ForEach(x =>
            {
                try
                {
                    var tn = new TenderAgrokomplexNew("АО «Агрокомплекс»", "http://www.zao-agrokomplex.ru/purchase", 65,
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

        private void ParsingList()
        {
            _driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//lot"));
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
            var purNum = t.FindElementWithoutException(By.XPath(".//div[@class = 'lot__number']"))?.Text
                             .Replace("№", "").Trim() ??
                         throw new Exception("cannot find purNum");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var href = "https://atp.agrokomplex.ru/lots/detail/" + purNum;
            var purName = t.FindElementWithoutException(By.XPath(".//div[@class = 'lot__title']"))?.Text.Trim() ??
                          throw new Exception("cannot find purName");
            var orgName = "АО «Агрокомплекс»";
            var contactPerson =
                t.FindElementWithoutException(By.XPath(".//div[. = 'Ответственный']/following-sibling::div"))?.Text
                    .Trim() ??
                "";
            var phone = "";
            var datePubT =
                t.FindElementWithoutException(By.XPath(".//div[. = 'Дата публикации']/following-sibling::div"))?.Text
                    .Trim() ??
                "";
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                t.FindElementWithoutException(By.XPath(".//div[. = 'Крайний срок приема']/following-sibling::div"))
                    ?.Text.Trim() ??
                "";
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
                return;
            }

            var tp = new TypeAgrokomplex
            {
                OrgName = orgName,
                DateEnd = dateEnd,
                DatePub = datePub,
                Href = href,
                PurNum = purNum,
                PurName = purName,
                ContactPerson = contactPerson,
                Phone = phone
            };
            _urls.Add(tp);
        }

        private void Auth()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl("https://atp.agrokomplex.ru/login/authorization");
            Thread.Sleep(5000);
            _driver.SwitchTo().DefaultContent();
            _driver.FindElement(By.XPath("//input[@type = 'email']")).SendKeys(AppBuilder.AgroUser);
            _driver.FindElement(By.XPath("//input[@type = 'password']")).SendKeys(AppBuilder.AgroPass);
            _driver.FindElement(By.XPath("//button[@type = 'submit']")).Click();
            Thread.Sleep(5000);
        }
    }
}