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
    public class ParserUralair : ParserAbstract, IParser
    {
        private const int Count = 5;
        private const string Url = "https://www.uralairlines.ru/tenders/";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);

        public void Parsing()
        {
            Parse(ParsingUralair);
        }

        private void ParsingUralair()
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
                    "//tbody/tr[@class = 'uan-table-border__tr']")));
            ParsingList();
            for (var i = 1; i < Count; i++)
            {
                try
                {
                    //_driver.FindElement(By.XPath($"//ul[contains(@class, 'uk-pagination')]//a[. = '{i}']")).Click();
                    _driver.ExecutorJs(
                        $"var elem = document.querySelectorAll('ul.uk-pagination a'); elem[{i}].click()");
                    Thread.Sleep(5000);
                    wait.Until(dr =>
                        dr.FindElement(By.XPath(
                            "//tbody/tr[@class = 'uan-table-border__tr']")));
                    ParsingList();
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingList()
        {
            _driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//tbody/tr[@class = 'uan-table-border__tr']"));
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
            var purName = t.FindElementWithoutException(By.XPath("./td[1]"))?.Text.Replace("Название", "").Trim() ??
                          throw new Exception("cannot find purName " + t.Text);
            var href = "https://www.uralairlines.ru/tenders/";
            var purNum = purName.GetDataFromRegex(@"^(\d+)\s+");
            if (purNum == "")
            {
                Log.Logger("Empty purNum");
                return;
            }

            var status = t.FindElementWithoutException(By.XPath("./td[4]"))?.Text.Replace("Статус", "").Trim() ?? "";
            var datePubT = t.FindElementWithoutException(By.XPath("./td[3]/span[1]"))?.Text.Trim() ??
                           throw new Exception("cannot find datePubT");
            var datePub = datePubT.ParseDateUn("с dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndT = t.FindElementWithoutException(By.XPath("./td[3]/span[2]"))?.Text.Trim() ??
                           throw new Exception("cannot find dateEndT");
            var dateEnd = dateEndT.ParseDateUn("до dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var attachments = new List<TypeUralair.Attachment>();
            var atts = t.FindElements(By.XPath(@".//td[2]//a"));
            foreach (var a in atts)
            {
                var name = a.Text.Trim();
                var url = a.GetAttribute("href").Trim();
                if (name == "" || url == "") continue;
                attachments.Add(new TypeUralair.Attachment {Name = name, Url = url});
            }

            var tn = new TenderUralair("Уральские авиалинии", "https://www.uralairlines.ru/", 323,
                new TypeUralair
                {
                    PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                    Attachments = attachments, Status = status
                });
            ParserTender(tn);
        }
    }
}