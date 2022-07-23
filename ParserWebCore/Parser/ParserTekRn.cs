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
    public class ParserTekRn : ParserAbstract, IParser
    {
        private readonly ChromeDriver _driver = CreateChomeDriverNoHeadless.GetChromeDriver();
        private readonly List<TenderTekRn> tenderList = new List<TenderTekRn>();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);
        private int DateMinus => 30;

        public void Parsing()
        {
            try
            {
                Parse(ParsingTekRn);
                Parse(ParsingTekRnTkp);
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

        private void parsingList(TenderTekRn t)
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

        private void ParsingTekRn()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * DateMinus * 24 * 60);
            var urlStart =
                $"https://www.tektorg.ru/rosneft/procedures?dpfrom={dateM:dd.MM.yyyy}&limit=500&sort=datestart";

            GetPage(urlStart);
            tenderList.ForEach(parsingList);
            tenderList.Clear();
        }

        private void ParsingTekRnTkp()
        {
            var dateM = DateTime.Now.AddMinutes(-1 * DateMinus * 24 * 60);
            var urlStart =
                $"https://www.tektorg.ru/rosnefttkp/procedures?dpfrom={dateM:dd.MM.yyyy}&limit=500&sort=datestart";

            GetPage(urlStart, true);
            tenderList.ForEach(parsingList);
            tenderList.Clear();
        }

        private void GetPage(string urlStart, bool tektkp = false)
        {
            try
            {
                var wait = new WebDriverWait(_driver, _timeoutB);
                _driver.Navigate().GoToUrl(urlStart);
                Thread.Sleep(5000);
                _driver.SwitchTo().DefaultContent();
                wait.Until(dr =>
                    dr.FindElement(By.CssSelector(
                        "div.section-procurement__item")));
                ParsingPage(tektkp);
            }
            catch (Exception e)
            {
                Log.Logger(
                    $"Exception in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    e, urlStart);
            }
        }

        private void ParsingPage(bool tektkp = false)
        {
            _driver.SwitchTo().DefaultContent();
            var tens = _driver.FindElements(
                By.CssSelector(
                    "div.section-procurement__item"));
            foreach (var t in tens)
            {
                try
                {
                    ParsingTender(t, tektkp);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingTender(IWebElement t, bool tektkp = false)
        {
            var urlT = (t
                .FindElementWithoutException(By.XPath(
                    (".//a[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-title \")]")))
                ?.GetAttribute("href") ?? "").Trim();
            if (string.IsNullOrEmpty(urlT))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}");
            }

            var purName =
                (t.FindElementWithoutException(By.XPath(
                         ".//a[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-title \")]"))
                     ?.Text ??
                 "");

            var tenderUrl = urlT;
            if (!urlT.Contains("https://")) tenderUrl = $"https://www.tektorg.ru{urlT}";
            var status = (t
                    .FindElementWithoutException(By.XPath(
                        ".//div[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-dateTo \")][contains(normalize-space(),\"Статус:\")]"))
                    ?.Text
                    ?.Replace("Статус:", "") ?? "")
                .Trim();
            if (status.Contains("Осталось:"))
            {
                status = status.GetDataFromRegex("(.+)Осталось:.+").Trim();
            }

            var datePubT =
                (t.FindElementWithoutException(By.XPath(
                         ".//div[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-dateTo \")][contains(normalize-space(),\"Дата публикации процедуры:\")]"))
                     ?.Text ??
                 "").Replace("Дата публикации процедуры:", "").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger($"Empty dates in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    tenderUrl, datePubT);
                return;
            }

            var dateEndT =
                (t.FindElementWithoutException(By.XPath(
                         ".//div[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-dateTo \")][contains(normalize-space(),\"Дата окончания срока подачи технико-коммерческих частей\")]"))
                     ?.Text ??
                 "").Replace("Дата окончания срока подачи технико-коммерческих частей:", "").Trim();
            if (dateEndT == "")
            {
                dateEndT =
                    (t.FindElementWithoutException(By.XPath(
                             ".//div[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-dateTo \")][contains(normalize-space(),\"Дата окончания срока подачи коммерческих частей:\")]"))
                         ?.Text ??
                     "").Replace("Дата окончания срока подачи коммерческих частей:", "").Trim();
            }

            if (dateEndT == "")
            {
                dateEndT =
                    (t.FindElementWithoutException(By.XPath(
                             ".//div[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-dateTo \")][contains(normalize-space(),\"Дата окончания срока подачи технических частей:\")]"))
                         ?.Text ??
                     "").Replace("Дата окончания срока подачи технических частей:", "").Trim();
            }

            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");

            var purNum = (t
                .FindElementWithoutException(
                    By.XPath(".//div/span[contains(normalize-space(),\"Номер закупки на сайте ЭТП:\")]"))?.Text
                ?.Replace("Номер закупки на сайте ЭТП:", "") ?? "").Trim();
            if (purNum == "")
            {
                purNum =
                    (t.FindElementWithoutException(
                         By.XPath(".//span[contains(normalize-space(),\"Номер процедуры:\")]"))?.Text ??
                     "").Replace("Номер процедуры:", "").Trim();
            }

            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger($"Empty purNum in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                    tenderUrl);
                return;
            }

            var orgName = (t
                .FindElementWithoutException(By.XPath(
                    ".//div/span[contains(normalize-space(),\"Организатор:\")]/following-sibling::*[1]/self::a"))?.Text
                ?.Replace("Организатор:", "") ?? "").Trim();

            var dateScoringT =
                (t.FindElementWithoutException(By.XPath(
                         ".//div[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-dateTo \")][contains(normalize-space(),\"Подведение итогов не позднее:\")]"))
                     ?.Text ??
                 "").Replace("Подведение итогов не позднее:", "").Trim();
            var dateScoring = dateScoringT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");

            var nmckT = (t.FindElementWithoutException(By.XPath(
                    ".//div[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-totalPrice \")]"))
                ?.Text ?? "").Trim();
            var nmck = nmckT.ExtractPriceNew();
            if (tektkp)
            {
                var tn = new TenderTekRn("ТЭК Торг ТЭК Роснефть Запросы (Т)КП",
                    "https://www.tektorg.ru/rosneft/procedures", 362,
                    new TypeTekRn
                    {
                        Href = tenderUrl,
                        Status = status,
                        PurNum = purNum,
                        DatePub = datePub,
                        Scoring = dateScoring,
                        OrgName = orgName,
                        DateEnd = dateEnd,
                        Nmck = nmck,
                        PurName = purName,
                    }, _driver);
                tenderList.Add(tn);
            }
            else
            {
                var tn = new TenderTekRn("ТЭК Торг ТЭК Роснефть", "https://www.tektorg.ru/rosnefttkp/procedures", 149,
                    new TypeTekRn
                    {
                        Href = tenderUrl,
                        Status = status,
                        PurNum = purNum,
                        DatePub = datePub,
                        Scoring = dateScoring,
                        OrgName = orgName,
                        DateEnd = dateEnd,
                        Nmck = nmck,
                        PurName = purName,
                    }, _driver);
                tenderList.Add(tn);
            }
        }
    }
}