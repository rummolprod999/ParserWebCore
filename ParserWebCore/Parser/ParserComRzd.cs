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
    public class ParserComRzd : ParserAbstract, IParser
    {
        private const string Urlpage = "https://company.rzd.ru/ru/9395?f1465_pagesize=100";
        private const int Count = 10;
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private List<TypeComRzd> _listTenders = new List<TypeComRzd>();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);

        public void Parsing()
        {
            Parse(ParsingComRzd);
        }

        private void ParsingComRzd()
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
            _driver.Navigate().GoToUrl(Urlpage);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath("//a[@class = 'table2__row']")));
            ParserFirstPage();
            ParsingNextPage();
            ParserTenderList();
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
                        "var elem = document.querySelectorAll('a.pager__next'); elem[elem.length-1].click()");
                    Thread.Sleep(3000);
                    wait.Until(dr =>
                        dr.FindElement(
                            By.XPath("//a[@class = 'table2__row']")));
                    _driver.SwitchTo().DefaultContent();
                    ParserFirstPage();
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTenderList()
        {
            foreach (var tn in _listTenders)
            {
                var t = new TenderComRzd("ОАО «РЖД»", "https://company.rzd.ru/", 399,
                    tn, _driver);
                ParserTender(t);
            }
        }

        private void ParserFirstPage()
        {
            var tenders =
                _driver.FindElements(
                    By.XPath("//a[@class = 'table2__row']"));
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
            var purNum = t.FindElement(By.XPath("./div[@data-title='Номер процедуры']"))?.Text
                             ?.Replace("\u200a/\u200a", "/").Trim() ??
                         throw new Exception("cannot find purNum");
            var dateEndT = t.FindElement(By.XPath("./div[@data-title='Дата окончания подачи заявок']"))?.Text.Trim() ??
                           throw new Exception("cannot find dateEndT");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd " + purNum + " set curr date plus 2 days");

                dateEnd = DateTime.Today.AddDays(2);
            }

            var purName = t.FindElement(By.XPath("./div[@data-title='Наименование процедуры']"))?.Text.Trim() ??
                          throw new Exception("cannot find purName");
            var href = t.GetAttribute("href").Trim() ??
                       throw new Exception("cannot find href");
            var tt = new TypeComRzd
            {
                DateEnd = dateEnd,
                DatePub = DateTime.Now,
                Href = href,
                PurNum = purNum,
                PurName = purName
            };
            _listTenders.Add(tt);
        }
    }
}