using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using ParserWebCore.chrome;
using ParserWebCore.Creators;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserSportMaster : ParserAbstract, IParser
    {
        private const int Count = 5;
        private const string Url = "https://zakupki.sportmaster.ru/tender_list.php";
        private readonly ChromeDriver _driver = CreatorChromeDriverNotDetect.GetChromeDriver();

        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);
        private List<Tender> _urls = new List<Tender>();

        public void Parsing()
        {
            Parse(ParsingSportMaster);
        }

        private void ParsingSportMaster()
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
                    "//table[contains(@class, 'table')]/tbody/tr[contains(., 'Запрос')]")));
            ParsingList();
            for (var i = 0; i < Count; i++)
            {
                _driver.SwitchTo().DefaultContent();
                try
                {
                    wait.Until(dr =>
                        dr.FindElement(By.XPath("//span[contains(@class, 'page-item-arrow__next')]")));
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
                        "var elem = document.querySelectorAll('span.page-item-arrow__next'); elem[0].click()");
                    Thread.Sleep(5000);
                }
                catch (Exception)
                {
                    Log.Logger("This is last page, return");
                }

                wait.Until(dr =>
                    dr.FindElement(By.XPath(
                        "//table[contains(@class, 'table')]/tbody/tr[contains(., 'Запрос')]")));
                _driver.SwitchTo().DefaultContent();
                ParsingList();
            }

            _urls.ForEach(x =>
            {
                try
                {
                    CreateTender(x);
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
            _driver.Navigate().GoToUrl("https://zakupki.sportmaster.ru/auth/?login=yes");
            Thread.Sleep(5000);
            _driver.SwitchTo().DefaultContent();
            _driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.SportUser);
            _driver.FindElement(By.XPath("//input[@name = 'password']")).SendKeys(AppBuilder.SportPass);
            _driver.FindElement(By.XPath("//button[. = 'Отправить']")).Click();
            Thread.Sleep(5000);
        }

        private void ParsingList()
        {
            _driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//table[contains(@class, 'table')]/tbody/tr[contains(., 'Запрос')]"));
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
            var href = t.FindElementWithoutException(By.XPath(".//a"))?.GetAttribute("href").Trim() ??
                       throw new Exception("cannot find href");
            var purName = t.FindElementWithoutException(By.XPath(".//a"))?.Text.Trim() ??
                          throw new Exception("cannot find purName");
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            _urls.Add(new Tender { PurName = purName, Url = href });
        }

        private protected void CreateTender(Tender tender)
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(tender.Url);
            _driver.SwitchTo().DefaultContent();
            Thread.Sleep(5000);
            _driver.SwitchTo().DefaultContent();
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//h1")));
            _driver.SwitchTo().DefaultContent();
            var purName = tender.PurName;
            var purNum = tender.Url.GetDataFromRegex(@"id=(\d+)");
            var datePubT =
                _driver.FindElementWithoutException(By.XPath("//td[. = 'Дата начала:']/following-sibling::td"))?.Text
                    .Trim() ??
                DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm");
            var dateEndT =
                _driver.FindElementWithoutException(By.XPath("//td[. = 'Дата окончания: ']/following-sibling::td"))
                    ?.Text.Trim() ??
                DateTime.Now.AddDays(2).ToString("dd.MM.yyyy HH:mm");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var status = _driver.FindElementWithoutException(By.XPath("//div[@class = 'tender-icons']/i"))
                             ?.GetAttribute("title").Trim() ??
                         "";
            var attachments = _driver.FindElements(By.XPath("//div[@class = 'document-row-item']/a"));
            var attach = new Dictionary<string, string>();
            foreach (var a in attachments)
            {
                var attText = a.Text.Trim();
                var attUrl = a.GetAttribute("href").Trim();
                if (attText != "")
                {
                    attach[attText] = attUrl;
                }
            }

            var pwName = _driver.FindElementWithoutException(By.XPath("//h3[. = 'Тип этапа']/following-sibling::span"))
                ?.Text.Trim() ?? "";
            var tn = new TenderSportMaster("ООО «Спортмастер»", "http://zakupki.sportmaster.ru/", 216,
                new TypeSportmaster
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = tender.Url, DateEnd = dateEnd,
                    Status = status, Attach = attach, PwName = pwName
                });
            ParserTender(tn);
        }

        protected class Tender
        {
            public string Url { get; set; }
            public string PurName { get; set; }

            public override string ToString()
            {
                return $"{nameof(Url)}: {Url}, {nameof(PurName)}: {PurName}";
            }
        }
    }
}