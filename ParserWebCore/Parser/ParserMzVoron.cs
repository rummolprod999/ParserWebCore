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
    public class ParserMzVoron : ParserAbstract, IParser
    {
        private const int Count = 6;

        private const string Url =
            "https://goszakupki.govvrn.ru/mzvoron/GzwSP/NoticesGrid?ItemId=16&show_title=on&expanded=0";

        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private List<TypeMzVoron> _tendersList = new List<TypeMzVoron>();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(30);

        public void Parsing()
        {
            Parse(ParsingMzVoron);
        }

        private void ParsingMzVoron()
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

        private void ParserSelenium()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            Auth(_driver, wait);
            _driver.Navigate().GoToUrl(Url);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')][1]/table/tbody")));
            ParsingList();
            for (var i = 0; i < Count; i++)
            {
                try
                {
                    wait.Until(dr =>
                        dr.FindElement(By.XPath("//div[@class = 'page_container']/span[contains(@class, 'next')]")));
                }
                catch (Exception)
                {
                    Log.Logger("This is last page, return");
                    return;
                }

                _driver.ExecutorJs(
                    "var elem = document.querySelectorAll('div.page_container span.next'); elem[0].click()");
                Thread.Sleep(5000);
                wait.Until(dr =>
                    dr.FindElement(By.XPath(
                        "//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')][1]/table/tbody")));
                ParsingList();
            }
        }

        private void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            driver.Navigate()
                .GoToUrl(
                    "https://goszakupki.govvrn.ru/mzvoron/Login/Form?err=badlogged&ret=%2fmzvoron%2fProfile%2fGotoHomePage");
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//input[@name = 'login']")));
            Thread.Sleep(1000);
            driver.SwitchTo().DefaultContent();
            driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.KalugUser);
            driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(AppBuilder.KalugPass);
            driver.FindElement(By.XPath("//input[@value = 'Вход']")).Click();
            Thread.Sleep(5000);
            ParserGzwSp.AuthCookieValue = driver.Manage().Cookies.GetCookieNamed("ebudget").Value;
        }

        private void ParserListTenders()
        {
            foreach (var tt in _tendersList)
            {
                var tn = new TenderMzVoron("ПОРТАЛ МАЛЫХ ЗАКУПОК Воронежской области",
                    "https://goszakupki.govvrn.ru/", 80,
                    tt);
                ParserTender(tn);
            }
        }

        private void ParsingList()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            /*var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')]/table/tbody"));
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
            }*/

            for (var i = 1; i <= 10; i++)
            {
                var dd = 2;
                while (true)
                {
                    try
                    {
                        _driver.SwitchTo().DefaultContent();
                        try
                        {
                            wait.Until(dr =>
                                dr.FindElement(By.XPath(
                                    $"//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')][{i}]/table/tbody")));
                        }
                        catch (Exception)
                        {
                            goto Finish;
                        }

                        var t = _driver.FindElement(By.XPath(
                            $"//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')][{i}]/table/tbody"));
                        ParsingPage(t);
                        break;
                    }
                    catch (Exception e)
                    {
                        dd--;
                        if (dd != 0) continue;
                        Log.Logger(e);
                        break;
                    }
                }
            }

            Finish:
            Log.Logger("Last element on page");
        }

        private void ParsingPage(IWebElement t)
        {
            //_driver.SwitchTo().DefaultContent();
            var purName =
                t.FindElementWithoutException(By.XPath(".//span[. = 'Объект закупки']/following-sibling::span"))?.Text
                    .Trim() ??
                throw new Exception("cannot find purName ");
            var href = t.FindElementWithoutException(By.XPath(".//span[@class = 'regnumber']/a"))?.GetAttribute("href")
                           .Trim() ??
                       throw new Exception("cannot find href");
            var purNum = t.FindElementWithoutException(By.XPath(".//span[@class = 'regnumber']/a"))?.Text.Trim() ??
                         throw new Exception("cannot find purNum ");
            var datePubT =
                t.FindElementWithoutException(By.XPath(".//span[. = 'Дата публикации']/following-sibling::span"))?.Text
                    .Trim() ??
                throw new Exception("cannot find datePubT");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndT =
                t.FindElementWithoutException(By.XPath(".//span[. = 'Период подачи заявок']/following-sibling::span"))
                    ?.Text.Trim() ??
                throw new Exception("cannot find dateEndT");
            dateEndT = dateEndT.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4}\s*\d{2}:\d{2})$").DelDoubleWhitespace();
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd");
            }

            var status =
                t.FindElementWithoutException(By.XPath(".//td/span[contains(@class, 'status')]"))?.Text.Trim() ??
                "";
            var cusName = t.FindElementWithoutException(By.XPath(".//span[@class = 'customer']/a"))?.Text.Trim() ?? "";
            var cusInn = t.FindElementWithoutException(By.XPath(".//span[@class = 'customer']/following-sibling::span"))
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