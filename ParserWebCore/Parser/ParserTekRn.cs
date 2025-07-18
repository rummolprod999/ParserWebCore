#region

using System;
using System.Collections.Generic;
using System.Reflection;
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
    public class ParserTekRn : ParserAbstract, IParser
    {
        private readonly ChromeDriver _driver = CreateChomeDriverNoHeadless.GetChromeDriver();
        private readonly List<TenderTekRn> tenderList = new List<TenderTekRn>();
        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(120);
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
            for (var i = 1; i < 50; i++)
            {
                var urlStart =
                    $"https://www.tektorg.ru/rosneft/procedures?page={i}&sort=datePublished_desc";

                GetPage(urlStart);
            }

            tenderList.ForEach(parsingList);
            tenderList.Clear();
        }

        private void ParsingTekRnTkp()
        {
            for (var i = 1; i < 50; i++)
            {
                var urlStart =
                    $"https://www.tektorg.ru/rosnefttkp/procedures?page={i}&sort=datePublished_desc";

                GetPage(urlStart);
            }

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
                    dr.FindElement(By.XPath(
                        "//div[contains(@class, 'sc-8d381391-0')]")));
                ParsingPage(tektkp);
            }
            catch (Exception e)
            {
                Log.Logger(
                    $"Exception in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    e, urlStart);
            }
        }

        private void ParsingPage(bool tektkp = false)
        {
            _driver.SwitchTo().DefaultContent();
            var tens = _driver.FindElements(
                By.XPath(
                    "//div[contains(@class, 'sc-8d381391-0')]"));
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
                    ".//a[contains(@class, 'CardProcedureViewstyled__Title')]"))
                ?.GetAttribute("href") ?? "").Trim();
            if (string.IsNullOrEmpty(urlT))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}");
            }

            var purName =
                t.FindElementWithoutException(By.XPath(
                        ".//a[contains(@class, 'CardProcedureViewstyled__Title')]"))
                    ?.Text ??
                "";

            var tenderUrl = urlT;
            if (!urlT.Contains("https://"))
            {
                tenderUrl = $"https://www.tektorg.ru{urlT}";
            }

            var status = (t
                    .FindElementWithoutException(By.XPath(
                        ".//span[contains(@class, 'ProcedureStatusstyled__StatusValue')]"))
                    ?.Text
                    ?.Replace("Статус:", "") ?? "")
                .Trim();
            if (status.Contains("Осталось:"))
            {
                status = status.GetDataFromRegex("(.+)Осталось:.+").Trim();
            }

            var datePubT =
                (t.FindElementWithoutException(By.XPath(
                         ".//div[. = 'Дата публикации']/following-sibling::time"))
                     ?.Text ??
                 "").Replace("\n", " ").Trim();
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");
            if (datePub == DateTime.MinValue)
            {
                Log.Logger($"Empty dates in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    tenderUrl, datePubT);
                return;
            }

            var dateEndT =
                (t.FindElementWithoutException(By.XPath(
                         ".//div[. = 'Дата окончания срока подачи технико-коммерческих частей']/following-sibling::time"))
                     ?.Text ??
                 "").Replace("\n", " ").Trim();
            if (dateEndT == "")
            {
                dateEndT =
                    (t.FindElementWithoutException(By.XPath(
                             ".//div[. = 'Дата окончания срока подачи коммерческих частей']/following-sibling::time"))
                         ?.Text ??
                     "").Replace("\n", " ").Trim();
            }

            if (dateEndT == "")
            {
                dateEndT =
                    (t.FindElementWithoutException(By.XPath(
                             ".//div[. = 'Дата окончания срока подачи технических частей']/following-sibling::time"))
                         ?.Text ??
                     "").Replace("\n", " ").Trim();
            }

            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");

            var purNum = (t
                .FindElementWithoutException(
                    By.XPath(".//span[contains(@class, 'ProcedureInfoHeaderstyled__RegistryNumber')]"))?.Text
                ?.Replace("№", "") ?? "").Trim();
            if (purNum == "")
            {
                purNum =
                    (t.FindElementWithoutException(
                         By.XPath(".//span[contains(normalize-space(),\"Номер процедуры:\")]"))?.Text ??
                     "").Replace("Номер процедуры:", "").Trim();
            }

            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger($"Empty purNum in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    tenderUrl);
                return;
            }

            var orgName = (t
                .FindElementWithoutException(By.XPath(
                    ".//div[. = 'Организатор']/following-sibling::div"))?.Text
                ?.Replace("Организатор:", "") ?? "").Trim();

            var dateScoringT =
                (t.FindElementWithoutException(By.XPath(
                         ".//div[contains(concat(\" \",normalize-space(@class),\" \"),\" section-procurement__item-dateTo \")][contains(normalize-space(),\"Подведение итогов не позднее:\")]"))
                     ?.Text ??
                 "").Replace("Подведение итогов не позднее:", "").Trim();
            var dateScoring = dateScoringT.ParseDateUn("dd.MM.yyyy HH:mm 'GMT'z");

            var nmckT = (t.FindElementWithoutException(By.XPath(
                    ".//div[contains(@class, 'ProcedurePricestyled__Container')]"))
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
                        PurName = purName
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
                        PurName = purName
                    }, _driver);
                tenderList.Add(tn);
            }
        }
    }
}