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

namespace ParserWebCore.Parser
{
    public class ParserNaftan : ParserAbstract, IParser
    {
        private const string StartUrl = "http://www.naftan.by/ru/Home/PivotKps";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(30);
        private List<TenderNaftan> _listTenders = new List<TenderNaftan>();

        public void Parsing()
        {
            Parse(ParsingNaftan);
        }

        private void ParsingNaftan()
        {
            try
            {
                GetCategories();
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

        private void GetCategories()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(StartUrl);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//a[@title and contains(@href, 'KpAnnounces')]")));
            var pages =
                _driver.FindElements(
                        By.XPath(
                            "//a[@title and contains(@href, 'KpAnnounces')]"))
                    .Select(t => t.GetAttribute("href").Trim())
                    .ToList();
            foreach (var href in pages)
            {
                try
                {
                    ParserSelenium(href);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }

            ParsingListTendersNaftan();
        }

        private void ParserSelenium(string url)
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(url);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//a[ contains(@href, 'pageNumber')]")));
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
            _driver.Clicker($"//a[ contains(@href, 'pageNumber')][. = '{i}']");
            Thread.Sleep(3000);
            _driver.SwitchTo().DefaultContent();
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//ul[@class = 'posts']/li")));
            ParsingList();
        }

        private void ParsingList()
        {
            Thread.Sleep(2000);
            _driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//ul[@class = 'posts']/li"));
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
            _driver.SwitchTo().DefaultContent();
            var purName =
                t.FindElement(By.XPath("(.//h5/strong)[last()]"))
                    ?.Text.Trim();
            var href = t
                .FindElement(By.XPath(".//h4/a"))
                ?.GetAttribute("href").Trim();
            var tmpPurNum =
                t.FindElementWithoutException(By.XPath(".//h4/a"))?.Text
                    .Trim() ?? throw new Exception($"bad tmpPurNum {href}");
            var purNum = tmpPurNum.GetDataFromRegex(@"№\s*([\d-]+)\b");
            if (purNum == "")
            {
                throw new Exception($"cannot find purNum {tmpPurNum}");
            }

            var datePubTmp = t.FindElement(By.XPath(".//div[@class = 'details']/span"))
                ?.Text.Trim();
            var datePub = datePubTmp.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndTmp = t.FindElement(By.XPath(".//p/span[@class = 'fs-small']"))
                ?.Text.Trim();
            var (tm, dt) = dateEndTmp.GetTwoDataFromRegex(@"(\d{2}:\d{2}).+(\d{2}\.\d{2}\.\d{4})");
            var dateEnd = $"{dt} {tm}".ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd");
                dateEnd = datePub.AddDays(2);
            }

            var tt = new TypeNaftan
                { DateEnd = dateEnd, DatePub = datePub, Href = href, PurName = purName, PurNum = purNum };
            var tn = new TenderNaftan("ОАО «Нафтан»", "http://www.naftan.by/", 118, tt, _driver);
            _listTenders.Add(tn);
        }

        private int GetLastNumPage()
        {
            var numText =
                _driver.FindElementWithoutException(By.XPath("//a[ contains(@href, 'pageNumber')][last()]"))
                    ?.Text.Trim() ?? "";
            int.TryParse(numText, out var res);
            return res;
        }
    }
}