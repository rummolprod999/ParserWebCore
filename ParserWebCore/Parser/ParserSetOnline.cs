using System;
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
    public class ParserSetOnline : ParserAbstract, IParser
    {
        private const int Count = 5;
        private const string Url = "https://etp.setonline.ru/app/SearchLots/page";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);

        public void Parsing()
        {
            Parse(ParsingSetOnline);
        }

        private void ParsingSetOnline()
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
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(Url);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//table[contains(@class, 'cellTableWidget')]//tbody/tr[starts-with(@class, 'cellTable')]")));
            ParsingList();
            for (var i = 0; i < Count; i++)
            {
                try
                {
                    wait.Until(dr =>
                        dr.FindElement(By.XPath("//div[@class = 'gwt-HTML']/a[. = '>']")));
                }
                catch (Exception)
                {
                    Log.Logger("This is last page, return");
                    return;
                }

                _driver.ExecutorJs(
                    "var elem = document.querySelectorAll('div.pages div.gwt-HTML'); elem[elem.length-2].click()");
                Thread.Sleep(5000);
                wait.Until(dr =>
                    dr.FindElement(By.XPath(
                        "//table[contains(@class, 'cellTableWidget')]//tbody/tr[starts-with(@class, 'cellTable')]")));
                ParsingList();
            }
        }

        private void ParsingList()
        {
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//table[contains(@class, 'cellTableWidget')]//tbody/tr[starts-with(@class, 'cellTable')]"));
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
            var purName = t.FindElementWithoutException(By.XPath("./td[3]/div/a"))?.Text.Trim() ??
                          throw new Exception($"cannot find purName {t.Text}");
            var href = t.FindElementWithoutException(By.XPath("./td[3]/div/a"))?.GetAttribute("href").Trim() ??
                       throw new Exception("cannot find href");
            var datePubT = t.FindElementWithoutException(By.XPath("./td[6]/div"))?.Text.Trim() ??
                           throw new Exception("cannot find datePubT");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndT = t.FindElementWithoutException(By.XPath("./td[7]/div"))?.Text.Trim() ??
                           throw new Exception("cannot find dateEndT");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd");
            }

            var orgName = t.FindElementWithoutException(By.XPath("./td[4]/div"))?.Text.Trim() ?? "";
            var pwName = t.FindElementWithoutException(By.XPath("./td[2]/div"))?.Text.Trim() ?? "";
            var nmcKt = t.FindElementWithoutException(By.XPath("./td[5]/div"))?.Text.Trim() ?? "";
            var nmcK = GetPriceFromString(nmcKt);
            var currency = nmcKt.GetDataFromRegex(@"^[\d \.]+\s([\. \p{IsCyrillic}]+)\s");
            var tt = new TypeSetOnline
            {
                OrgName = orgName,
                DateEnd = dateEnd,
                DatePub = datePub,
                Href = href,
                PurNum = "",
                PurName = purName,
                PwName = pwName,
                Nmck = nmcK,
                Currency = currency
            };
            var tn = new TenderSetOnline("ООО «СЭТ»", "https://etp.setonline.ru", 73,
                tt);
            ParserTender(tn);
        }
    }
}