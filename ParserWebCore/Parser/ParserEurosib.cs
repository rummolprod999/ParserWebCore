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
    public class ParserEurosib : ParserAbstract, IParser
    {
        private const string Urlpage = "https://www.eurosib-td.ru/ru/zakupki-rabot-i-uslug/";
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private readonly List<TypeEurosib> _listTenders = new List<TypeEurosib>();
        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(120);

        public void Parsing()
        {
            Parse(ParsingEurosib);
        }

        private void ParsingEurosib()
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
                dr.FindElement(By.XPath("//table[@id = 'z']//tr[contains(., 'Дата начала подачи заявок:')]")));
            ParserFirstPage();
            ParserTenderList();
        }

        private void ParserTenderList()
        {
            foreach (var tn in _listTenders)
            {
                var t = new TenderEurosib("ООО «Торговый дом «ЕвроСибЭнерго»", "https://www.eurosib-td.ru/", 329,
                    tn);
                ParserTender(t);
            }
        }

        private void ParserFirstPage()
        {
            var tenders =
                _driver.FindElements(
                    By.XPath("//table[@id = 'z']//tr[contains(., 'Дата начала подачи заявок:')]"));
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
            var purName = "";
            var purNum = t.FindElement(By.XPath(".//a"))?.Text.Replace("№", "").Trim() ??
                         throw new Exception("cannot find purNum");
            var href =
                t.FindElement(By.XPath(".//a"))?.GetAttribute("href") ??
                throw new Exception(
                    $"Cannot find href in {purNum}");
            var cusName = "";
            var pubDateT =
                t.FindElement(By.XPath(".//p[contains(., 'Дата начала подачи заявок:')]"))?.Text
                    ?.Replace("Дата начала подачи заявок:", "")
                    .Trim().ReplaceHtmlEntyty().DelDoubleWhitespace().Trim() ??
                throw new Exception("cannot find pubDateT");
            var datePub = pubDateT.ParseDateUn("dd/MM/yyyy HH:mm");
            if (datePub == DateTime.MinValue)
            {
                datePub = pubDateT.ParseDateUn("dd/MM/yyyy");
            }

            if (datePub == DateTime.MinValue)
            {
                datePub = DateTime.Today;
            }

            var endDateT =
                t.FindElement(By.XPath(".//p[contains(., 'Дата окончания приема заявок:')]"))?.Text
                    ?.Replace("Дата окончания приема заявок:", "")
                    .Trim().ReplaceHtmlEntyty().DelDoubleWhitespace().Trim() ?? "";
            var dateEnd = endDateT.ParseDateUn("dd/MM/yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = endDateT.ParseDateUn("dd/MM/yyyy");
            }

            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = endDateT.ParseDateUn("dd/MM/yyyy HH:mm:ss");
            }

            if (dateEnd == DateTime.MinValue)
            {
                datePub.AddDays(2);
            }

            var tn = new TypeEurosib
            {
                PurName = purName, PurNum = purNum, DatePub = datePub, Href = href, DateEnd = dateEnd,
                CusName = cusName
            };
            _listTenders.Add(tn);
        }
    }
}