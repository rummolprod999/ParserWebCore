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
    public class ParserNaftan : ParserAbstract, IParser
    {
        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(30);
        private const string StartUrl = "http://www.naftan.by/ru/all_tenders.aspx?KindGoodId=-1";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private List<TenderNaftan> _listTenders = new List<TenderNaftan>();

        public void Parsing()
        {
            Parse(ParsingNaftan);
        }

        private void ParsingNaftan()
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
                _driver.Close();
                _driver.Quit();
            }
        }

        private void ParserSelenium()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(StartUrl);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//td[. = 'Страница:']/following-sibling::td[last()]")));
            var lastPage = GetLastNumPage();
            ParsingList();
            if (lastPage != 0)
            {
                for (var i = 2; i <= lastPage; i++)
                {
                    try
                    {
                        ParsingNextPage(i, wait);
                    }
                    catch (Exception e)
                    {
                        Log.Logger(e);
                    }
                }
            }
            else
            {
                Log.Logger("cannot find last pages num");
            }

            ParsingListTendersNaftan();
        }

        private void ParsingListTendersNaftan()
        {
            foreach (var t in _listTenders)
            {
                try
                {
                    ParserTender(t);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingNextPage(int i, WebDriverWait wait)
        {
            _driver.Clicker($"//td[. = 'Страница:']/following-sibling::td[. = '{i}']");
            Thread.Sleep(3000);
            _driver.SwitchTo().DefaultContent();
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//td[@id = 'First_NewsControl1_ICell']/table/tbody/tr")));
            ParsingList();
        }

        private void ParsingList()
        {
            Thread.Sleep(3000);
            _driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//td[@id = 'First_NewsControl1_ICell']/table/tbody/tr"));
            foreach (var t in tenders)
            {
                try
                {
                    if (t is null)
                    {
                        Console.WriteLine("null");
                        continue;
                    }

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
            _driver.SwitchTo().DefaultContent();
            var purName =
                t.FindElement(By.XPath(".//div[contains(@class, 'dxncItemContent')]/a[starts-with(@href, 'one')]"))
                    ?.Text.Trim();
            var href = t
                .FindElement(By.XPath(".//div[contains(@class, 'dxncItemContent')]/a[starts-with(@href, 'one')]"))
                ?.GetAttribute("href").Trim();
            var tmpPurNum = t.FindElementWithoutException(By.XPath(".//div[contains(@class, 'dxncItemHeader')]/span"))?.Text.Trim() ?? throw new Exception($"bad tmpPurNum {href}");
            var purNum = tmpPurNum.GetDataFromRegex(@"№\s*([\d-]+)\b");
            if (purNum == "")
            {
                throw new Exception($"cannot find purNum {tmpPurNum}");
            }

            var datePubTmp = t.FindElement(By.XPath(".//div[contains(@class, 'dxncItemDate')]"))
                ?.Text.Trim();
            var datePub = datePubTmp.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndTmp = t.FindElement(By.XPath(".//div[contains(@class, 'dxncItemContent')]"))
                ?.Text.Trim();
            var (tm, dt) = dateEndTmp.GetTwoDataFromRegex(@"(\d{2}:\d{2}).+(\d{2}\.\d{2}\.\d{4})");
            var dateEnd = $"{dt} {tm}".ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd");
            }

            var tt = new TypeNaftan
                {DateEnd = dateEnd, DatePub = datePub, Href = href, PurName = purName, PurNum = purNum};
            var tn = new TenderNaftan("ОАО «Нафтан»", "http://www.naftan.by/", 118, tt);
            _listTenders.Add(tn);
        }

        private int GetLastNumPage()
        {
            var numText =
                _driver.FindElementWithoutException(By.XPath("//td[. = 'Страница:']/following-sibling::td[last()]"))
                    ?.Text.Trim() ?? "";
            int.TryParse(numText, out var res);
            return res;
        }
    }
}