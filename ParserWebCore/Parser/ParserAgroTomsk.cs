#region

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

#endregion

namespace ParserWebCore.Parser
{
    public class ParserAgroTomsk : ParserAbstract, IParser
    {
        private const int Count = 5;
        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(120);
        private const string Url = "http://agro.zakupki.tomsk.ru/Competition/Competition_Request_Cost.aspx?Sale=0";
        private readonly List<TypeAgroTomsk> _listTenders = new List<TypeAgroTomsk>();
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();

        public void Parsing()
        {
            Parse(ParsingAgroTomsk);
        }

        private void ParsingAgroTomsk()
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
                dr.FindElement(By.XPath("//table[@id = 'MainContent_dgProducts']//tr[contains(@class, 'ltint')][20]")));
            ParserFirstPage();
            ParsingNextPage();
            ParserTenderList();
        }

        private void ParserTenderList()
        {
            foreach (var tn in _listTenders)
            {
                var t = new TenderAgroTomsk("ЭТП ЗАО \"Сибирская Аграрная Группа\"", "http://agro.zakupki.tomsk.ru", 68,
                    tn);
                ParserTender(t);
            }
        }

        private void ParserFirstPage()
        {
            var tenders =
                _driver.FindElements(
                    By.XPath("//table[@id = 'MainContent_dgProducts']//tr[contains(@class, 'ltint')]"));
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

        private void ParsingNextPage()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            for (var i = 1; i <= Count; i++)
            {
                try
                {
                    //_driver.Clicker("//td[contains(., 'Перейти на страницу:')]//a[. = '>']");
                    _driver.ExecutorJs(
                        "var elem = document.querySelectorAll('#MainContent_dgProducts > tbody > tr:nth-child(1) > td > a.NotVisitedLink'); elem[elem.length-1].click()");
                    Thread.Sleep(5000);
                    wait.Until(dr =>
                        dr.FindElement(
                            By.XPath("//table[@id = 'MainContent_dgProducts']//tr[contains(@class, 'ltint')][20]")));
                    _driver.SwitchTo().DefaultContent();
                    ParserFirstPage();
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
            //var Num = t.FindElement(By.XPath("./td[1]"))?.Text.Trim() ?? throw new Exception("cannot find Num");
            var purNum = t.FindElement(By.XPath("./td[3]"))?.Text.Trim() ?? throw new Exception("cannot find purNum");
            var datePubT = t.FindElement(By.XPath("./td[4]"))?.Text.Trim() ??
                           throw new Exception("cannot find datePubT");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndT = t.FindElement(By.XPath("./td[6]"))?.Text.Trim() ??
                           throw new Exception("cannot find dateEndT");
            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd");
            }
            else
            {
                dateEnd = dateEnd.AddHours(-4);
            }

            var purName = t.FindElement(By.XPath("./td[5]/a/span"))?.Text.Trim() ??
                          throw new Exception("cannot find purName");
            var href = t.FindElement(By.XPath("./td[5]/a"))?.GetAttribute("href").Trim() ??
                       throw new Exception("cannot find href");
            var orgName = t.FindElement(By.XPath("./td[7]/a/span"))?.Text.Trim() ?? "";
            var pwName = t.FindElement(By.XPath("./td[8]"))?.Text.Trim() ?? "";
            var status = t.FindElement(By.XPath("./td[9]"))?.Text.Trim() ?? "";
            var tt = new TypeAgroTomsk
            {
                OrgName = orgName,
                DateEnd = dateEnd,
                DatePub = datePub,
                Href = href,
                PurNum = purNum,
                PurName = purName,
                PwName = pwName,
                Status = status
            };
            _listTenders.Add(tt);
        }
    }
}