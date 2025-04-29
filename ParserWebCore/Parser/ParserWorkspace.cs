#region

using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class ParserWorkspace : ParserAbstract, IParser
    {
        private const string StartUrl = "https://workspace.ru/tenders/";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();

        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(30);
        private readonly List<TenderWorkspace> _listTenders = new List<TenderWorkspace>();

        public void Parsing()
        {
            Parse(ParsingWorkspace);
        }

        private void ParsingWorkspace()
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
            _driver.SwitchTo().DefaultContent();
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//div[@class = 'vacancies__card _tender'][10]")));
            ParsingList();
            ParsingListTenders();
        }

        private void ParsingList()
        {
            Thread.Sleep(3000);
            LoadList();
            _driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//div[@class = 'vacancies__card _tender']"));
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

        private void LoadList()
        {
            for (var i = 0; i < 50; i++)
            {
                try
                {
                    _driver.SwitchTo().DefaultContent();
                    var wait = new WebDriverWait(_driver, _timeoutB);
                    wait.Until(dr =>
                        dr.FindElement(By.XPath(
                            "//a[contains(., 'Показать ещё')]")));
                    _driver.SwitchTo().DefaultContent();
                    _driver.FindElement(By.XPath("//a[contains(., 'Показать ещё')]")).Click();
                    Thread.Sleep(2000);
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
                t.FindElement(By.XPath(".//div[@class = 'b-tender__title _wide']/a"))
                    ?.Text.Trim();
            if (string.IsNullOrEmpty(purName))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var href = t
                .FindElement(By.XPath(".//div[@class = 'b-tender__title _wide']/a"))
                ?.GetAttribute("href").Trim();
            if (string.IsNullOrEmpty(href))
            {
                Log.Logger("Empty href");
                return;
            }

            var purNum = href.GetDataFromRegex(@"(\d+)/$");
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var datePubTmp = t.FindElement(By.XPath(".//div[. = 'Опубликован']/following-sibling::div"))
                ?.Text.Trim();
            var myCultureInfo = new CultureInfo("ru-RU");
            var datePub = DateTime.Parse(datePubTmp, myCultureInfo);
            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndTmp = t.FindElement(By.XPath(".//div[. = 'Крайний срок приема заявок:']/following-sibling::div"))
                ?.Text.Trim();
            var dateEnd = DateTime.Parse(dateEndTmp, myCultureInfo);
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = datePub.AddDays(2);
            }

            var status = t.FindElement(By.XPath(".//div[contains(@class, 'tendercart-type__status-public')]"))
                ?.Text.Trim();
            var nmck = t.FindElement(By.XPath(".//div[@class = 'b-tender__info-item-text']"))
                ?.Text.Trim().DelAllWhitespace();
            nmck = nmck.GetDataFromRegex(@"(\d+)$");
            var tt = new TypeWorcspace
            {
                DateEnd = dateEnd, DatePub = datePub, Href = href, PurName = purName, PurNum = purNum, Status = status,
                Nmck = nmck
            };
            var tn = new TenderWorkspace("WORKSPACE", "https://workspace.ru/", 316, tt);
            _listTenders.Add(tn);
        }

        private void ParsingListTenders()
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
    }
}