using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserFbRts : ParserAbstract, IParser
    {
        private const int Count = 30;
        private readonly ChromeDriver _driver;

        private readonly string[] Url =
        {
            "https://fb.rts-tender.ru/trades",
        };

        private List<TenderFbRts> _tendersList = new List<TenderFbRts>();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(60);

        public ParserFbRts()
        {
            var options = new ChromeOptions();
            options.AddArguments("headless");
            options.AddArguments("disable-gpu");
            options.AddArguments("no-sandbox");
            options.AddArguments("disable-infobars");
            options.AddArguments("lang=ru, ru-RU");
            options.AddArguments("window-size=1920,1080");
            options.AddArguments("disable-blink-features=AutomationControlled");
            options.AddArguments(
                "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.61 Safari/537.36");
            options.AddAdditionalCapability("useAutomationExtension", false);
            options.AddExcludedArgument("enable-automation");
            _driver = new ChromeDriver("/usr/local/bin", options);
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
            //Driver.Manage().Window.Maximize();
            _driver.Manage().Cookies.DeleteAllCookies();
        }

        public void Parsing()
        {
            try
            {
                Parse(ParsingRts);
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }
        }

        private void ParsingRts()
        {
            try
            {
                foreach (var s in Url)
                {
                    ParserSelenium(s);
                }
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

            ParserListTenders();
        }

        private void ParserSelenium(string Url)
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(Url);
            Thread.Sleep(5000);
            _driver.SwitchTo().DefaultContent();
            _driver.FindElement(By.XPath("//input[@name = 'Username']")).SendKeys(AppBuilder.FbRtsUser);
            _driver.FindElement(By.XPath("//button[. = 'Далее']")).Click();
            Thread.Sleep(1000);
            _driver.SwitchTo().DefaultContent();
            _driver.FindElement(By.XPath("//input[@name = 'Password']")).SendKeys(AppBuilder.FbRtsPass);
            _driver.FindElement(By.XPath("//button[. = 'Войти']")).Click();
            wait.Until(dr =>
                dr.FindElement(
                    By.XPath("//article[@class = 'trade-card']")));
            _driver.SwitchTo().DefaultContent();
            ParserFirstPage();
            ParsingNextPage();
        }

        private void ParserListTenders()
        {
            foreach (var tenderFbRts in _tendersList)
            {
                tenderFbRts.ParsingTender();
            }
        }

        private void ParsingNextPage()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            for (var i = 1; i <= Count; i++)
            {
                try
                {
                    //_driver.Clicker("//td[contains(., 'Перейти на страницу:')]//a[. = '>']");
                    _driver.ExecutorJs(
                        "var elem = document.querySelectorAll('a.ui-paginator-next'); elem[elem.length-1].click()");
                    Thread.Sleep(1000);
                    wait.Until(dr =>
                        dr.FindElement(
                            By.XPath("//article[@class = 'trade-card']")));
                    _driver.SwitchTo().DefaultContent();
                    ParserFirstPage();
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserFirstPage()
        {
            var tenders =
                _driver.FindElements(
                    By.XPath("//article[@class = 'trade-card']"));
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
            var purNum = t.FindElement(By.XPath(".//a[@class = 'link']"))?.Text
                             ?.Replace("\u2116", "").Trim() ??
                         throw new Exception("cannot find purNum");
            var printForm = t.FindElementWithoutException(By.XPath(".//a[@class = 'link']"))?.GetAttribute("href")
                                .Trim() ??
                            throw new Exception("cannot find href");
            var href = t.FindElementWithoutException(By.XPath(".//a[.='Подробнее']"))?.GetAttribute("href").Trim() ??
                       throw new Exception("cannot find href");
            var purName = t.FindElementWithoutException(By.XPath(".//a[contains(@class, 'main-text')]"))?.Text.Trim() ??
                          throw new Exception("cannot find purName");
            var datePubT =
                t.FindElementWithoutException(By.XPath(".//span[contains(., 'Опубликовано')]"))?.Text
                    .Replace("Опубликовано", "").Replace("МСК", "").GetDateWithMonthFull().Trim() ??
                "";
            var datePub = datePubT.ParseDateUn("d MM yyyy HH:mm");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub", href);
                return;
            }

            var dateEndT =
                t.FindElementWithoutException(By.XPath(".//span[contains(., 'Осталось')]/following-sibling::span"))
                    ?.Text
                    .Replace("до", "").Replace("МСК", "").GetDateWithMonthNewDot().Trim() ??
                "";
            var dateEnd = dateEndT.ParseDateUn("d MM yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var status =
                t.FindElementWithoutException(By.XPath(".//span[contains(., 'Статус')]/following-sibling::p"))?.Text
                    .Trim() ??
                "";
            var nmck =
                t.FindElementWithoutException(By.XPath(".//p[contains(., 'Сумма НМЦ')]/following-sibling::p"))?.Text
                    .Replace("\u20bd", "").ExtractPriceNew().Trim() ??
                "";
            var orgName =
                t.FindElementWithoutException(By.XPath(".//p[contains(., 'Организатор')]/following-sibling::p"))?.Text
                    .Trim() ??
                "";
            var cusName =
                t.FindElementWithoutException(By.XPath(".//p[contains(., 'Заказчик')]/following-sibling::p"))?.Text
                    .Trim() ??
                "";
            var region =
                t.FindElementWithoutException(By.XPath(".//p[contains(., 'Регион поставки')]/following-sibling::p"))
                    ?.Text
                    .Trim() ??
                "";
            t.FindElement(By.XPath("./app-trades-card-details")).Click();
            Thread.Sleep(500);
            var positions =
                t.FindElements(
                    By.XPath(".//div[@class = 'table']//div[contains(@class, 'table__content')]"));
            var pos = new List<TypeFbRtsPos>();
            foreach (var p in positions)
            {
                var name = p.FindElementWithoutException(By.XPath("./span[1]"))?.Text
                               .Trim() ??
                           "";
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var PQ = p.FindElementWithoutException(By.XPath(".//span[2]"))?.Text
                             .Trim() ??
                         "";
                var quant = PQ.ExtractPriceNew();
                var okei = PQ.GetDataFromRegex(@"\s+(.+)$");
                var price =
                    p.FindElementWithoutException(By.XPath(".//span[3]"))?.Text
                        .Replace("\u20bd", "").ExtractPriceNew().Trim() ??
                    "";
                var sum =
                    p.FindElementWithoutException(By.XPath(".//span[4]"))?.Text
                        .Replace("\u20bd", "").ExtractPriceNew().Trim() ??
                    "";
                var obj = new TypeFbRtsPos() { Quantity = quant, Name = name, Okei = okei, Price = price, Sum = sum };
                pos.Add(obj);
            }

            var type = new TypeFbRts()
            {
                CusName = cusName, PurName = purName, PurNum = purNum, Status = status, positions = pos, Href = href,
                Nmck = nmck, DateEnd = dateEnd, DatePub = datePub, OrgName = orgName, Region = region,
                PrintForm = printForm
            };
            var td = new TenderFbRts("РТС", "https://fb.rts-tender.ru/", 406, type);
            _tendersList.Add(td);
        }
    }
}