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
    public class ParserGzwSp : ParserAbstract, IParser
    {
        public ParserGzwSp(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg)
        {
            _url = url;
            _baseUrl = baseurl;
            _etpName = etpName;
            _etpUrl = etpUrl;
            _typeFz = typeFz;
            _arg = arg;
        }

        private const int Count = 10;
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(30);
        private string _url;
        private string _baseUrl;
        private string _etpName;
        private string _etpUrl;
        private int _typeFz;
        private Arguments _arg;
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private List<TypeMzVoron> _tendersList = new List<TypeMzVoron>();

        public void Parsing()
        {
            Parse(ParsingGzwSp);
        }

        private void ParsingGzwSp()
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
            _driver.Navigate().GoToUrl(_url);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')][1]/table/tbody")));
            ParsingList(1);
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
                ParsingList(i);
            }
        }

        private void ParserListTenders()
        {
            foreach (var tt in _tendersList)
            {
                var tn = new TenderGzwSp(_etpName,
                    _etpUrl, _typeFz,
                    tt, _baseUrl, _arg);
                ParserTender(tn);
            }
        }

        private void ParsingList(int pageNum)
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
                        wait.Until(dr =>
                            dr.FindElement(By.XPath(
                                $"//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')][{i}]/table/tbody")));
                        var t = _driver.FindElementByXPath(
                            $"//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')][{i}]/table/tbody");
                        ParsingPage(t);
                        break;
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("Timed out after"))
                        {
                            Log.Logger($"find the last tender number {i} on page {pageNum + 2}");
                            return;
                        }

                        dd--;
                        if (dd != 0) continue;
                        Log.Logger(e);
                        break;
                    }
                }
            }
        }

        private void ParsingPage(IWebElement t)
        {
            //_driver.SwitchTo().DefaultContent();
            var purName =
                t.FindElementWithoutException(By.XPath(".//span[. = 'Объект закупки']/following-sibling::span"))?.Text
                    .Trim() ?? "";
            if (string.IsNullOrEmpty(purName))
            {
                purName =
                    t.FindElementWithoutException(
                            By.XPath(".//span[. = 'Объект исследования']/following-sibling::span"))?.Text
                        .Trim() ??
                    throw new Exception("cannot find purName ");
            }

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
                datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm");
            }

            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndTt =
                t.FindElementWithoutException(By.XPath(".//span[. = 'Период подачи заявок']/following-sibling::span"))
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