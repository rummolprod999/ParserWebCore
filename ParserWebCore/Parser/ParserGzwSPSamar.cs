#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using ParserWebCore.Creators;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;
using TwoCaptcha.Captcha;
using Cookie = System.Net.Cookie;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserGzwSpSamar : ParserAbstract, IParser
    {
        private const int Count = 10;
        private readonly ChromeDriver _driver = CreatorChromeDriverNoSsl.GetChromeDriver();
        private readonly Arguments _arg;
        private readonly string _baseUrl;
        private readonly string _etpName;
        private readonly string _etpUrl;
        private readonly List<TypeMzVoron> _tendersList = new List<TypeMzVoron>();
        private readonly TimeSpan _timeoutB = TimeSpan.FromSeconds(30);
        private readonly int _typeFz;
        private readonly string _url;

        public ParserGzwSpSamar(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg)
        {
            _url = url;
            _baseUrl = baseurl;
            _etpName = etpName;
            _etpUrl = etpUrl;
            _typeFz = typeFz;
            _arg = arg;
        }

        public void Parsing()
        {
            Parse(ParsingGzwSp);
        }

        private void ParsingGzwSp()
        {
            try
            {
                ParserSelenium();
                ParserListTenders();
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
            Auth(_driver, wait);
            _driver.Navigate().GoToUrl(_url);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')][1]/table/tbody")));
            ParsingList(1);
            for (var i = 0; i < Count; i++)
            {
                try
                {
                    wait.Until(dr =>
                        dr.FindElement(By.XPath("//div[@class = 'page_container']/span[contains(@class, 'next')]")));
                }
                catch (Exception)
                {
                    Log.Logger("This is last page, return");
                    return;
                }

                _driver.ExecutorJs(
                    "var elem = document.querySelectorAll('div.page_container span.next'); elem[0].click()");
                Thread.Sleep(5000);
                wait.Until(dr =>
                    dr.FindElement(By.XPath(
                        "//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')][1]/table/tbody")));
                ParsingList(i);
            }
        }

        private void ParserListTenders()
        {
            foreach (var tt in _tendersList)
            {
                var tn = new TenderGzwSp(_etpName,
                    _etpUrl, _typeFz,
                    tt, _baseUrl, _arg);
                ParserTender(tn);
            }
        }

        public void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            var count = 3;
            while (count > 0)
            {
                try
                {
                    driver.Navigate()
                        .GoToUrl(
                            "https://webtorgi.samregion.ru//smallpurchases/Login/Form?err=badlogged&ret=%2fsmallpurchases%2fProfile%2fGotoHomePage");
                    wait.Until(dr =>
                        dr.FindElement(By.XPath(
                            "//input[@name = 'login']")));
                    //Thread.Sleep(1000);
                    driver.SwitchTo().DefaultContent();
                    driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.SamarUser);
                    driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(AppBuilder.SamarPass);
                    var solver = new TwoCaptcha.TwoCaptcha(AppBuilder.Api);
                    solver.DefaultTimeout = 120;
                    solver.RecaptchaTimeout = 600;
                    solver.PollingInterval = 10;
                    var base64string = driver.ExecuteScript(@"
    var c = document.createElement('canvas');
    var ctx = c.getContext('2d');
    var img = document.getElementById('captcha');
    c.height=img.naturalHeight;
    c.width=img.naturalWidth;
    ctx.drawImage(img, 0, 0,img.naturalWidth, img.naturalHeight);
    var base64String = c.toDataURL();
    return base64String;
    ") as string;

                    var base64 = base64string.Split(',').Last();
                    using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
                    {
                        using (var bitmap = new Bitmap(stream))
                        {
                            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Captcha.jpeg");
                            bitmap.Save(filepath, ImageFormat.Jpeg);
                        }
                    }

                    var captcha = new Normal();
                    captcha.SetFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Captcha.jpeg"));
                    captcha.SetMinLen(3);
                    captcha.SetMaxLen(20);
                    captcha.SetCaseSensitive(true);
                    solver.Solve(captcha).GetAwaiter().GetResult();
                    Console.WriteLine(captcha.Code);
                    driver.FindElement(By.XPath("//input[@name = 'captcha']")).SendKeys(captcha.Code);
                    driver.FindElement(By.XPath("//input[@value = 'Вход']")).Click();
                    Thread.Sleep(5000);
                    foreach (var cookiesAllCookie in driver.Manage().Cookies.AllCookies)
                    {
                        ParserGzwSp.col.Add(new Cookie(cookiesAllCookie.Name, cookiesAllCookie.Value));
                    }

                    break;
                }
                catch (Exception e)
                {
                    try
                    {
                        var alert = driver.SwitchTo().Alert();
                        alert.Accept();
                        _driver.Manage().Cookies.DeleteAllCookies();
                    }
                    catch (Exception)
                    {
                    }

                    count--;
                    Log.Logger(e);
                }
            }
        }

        private void ParsingList(int pageNum)
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            /*var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//div[@class = 'grid_content']/div[contains(@class, 'gridview_item')]/table/tbody"));
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
            }*/

            for (var i = 1; i <= 10; i++)
            {
                var dd = 2;
                while (true)
                {
                    try
                    {
                        _driver.SwitchTo().DefaultContent();
                        wait.Until(dr =>
                            dr.FindElement(By.XPath(
                                $"//div[@class = 'grid_content']//div[contains(@class, 'gridview_item')][{i}]/table/tbody")));
                        var t = _driver.FindElement(By.XPath(
                            $"//div[@class = 'grid_content']//div[contains(@class, 'gridview_item')][{i}]/table/tbody"));
                        ParsingPage(t);
                        break;
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("Timed out after"))
                        {
                            Log.Logger($"find the last tender number {i} on page {pageNum + 2}");
                            return;
                        }

                        dd--;
                        if (dd != 0)
                        {
                            continue;
                        }

                        Log.Logger(e);
                        break;
                    }
                }
            }
        }

        private void ParsingPage(IWebElement t)
        {
            //_driver.SwitchTo().DefaultContent();
            var purName =
                t.FindElementWithoutException(By.XPath(".//span[. = 'Объект закупки']/following-sibling::span"))?.Text
                    .Trim() ?? "";
            if (string.IsNullOrEmpty(purName))
            {
                purName =
                    t.FindElementWithoutException(
                            By.XPath(".//span[. = 'Объект исследования']/following-sibling::span"))?.Text
                        .Trim() ??
                    throw new Exception("cannot find purName ");
            }

            var href = t.FindElementWithoutException(By.XPath(".//span[@class = 'regnumber']/a"))?.GetAttribute("href")
                           .Trim() ??
                       throw new Exception("cannot find href");
            var purNum = t.FindElementWithoutException(By.XPath(".//span[@class = 'regnumber']/a"))?.Text.Trim() ??
                         throw new Exception("cannot find purNum ");
            var datePubT =
                t.FindElementWithoutException(By.XPath(".//span[. = 'Дата публикации']/following-sibling::span"))?.Text
                    .Trim() ??
                throw new Exception("cannot find datePubT");
            var datePub = datePubT.ParseDateUn("dd.MM.yyyy");
            if (datePub == DateTime.MinValue)
            {
                datePub = datePubT.ParseDateUn("dd.MM.yyyy HH:mm");
            }

            if (datePub == DateTime.MinValue)
            {
                Log.Logger("Empty datePub");
                return;
            }

            var dateEndTt =
                t.FindElementWithoutException(By.XPath(".//span[. = 'Период подачи заявок']/following-sibling::span"))
                    ?.Text.Trim() ??
                throw new Exception("cannot find dateEndT");
            var dateEndT = dateEndTt.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4}\s*\d{2}:\d{2})$").DelDoubleWhitespace();
            if (string.IsNullOrEmpty(dateEndT))
            {
                dateEndT = dateEndTt.GetDataFromRegex(@"(\d{2}\.\d{2}\.\d{4})$").DelDoubleWhitespace();
            }

            var dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy HH:mm");
            if (dateEnd == DateTime.MinValue)
            {
                dateEnd = dateEndT.ParseDateUn("dd.MM.yyyy");
            }

            if (dateEnd == DateTime.MinValue)
            {
                Log.Logger("Empty dateEnd", href);
            }

            var status =
                t.FindElementWithoutException(By.XPath(".//td/span[contains(@class, 'status')]"))?.Text.Trim() ??
                "";
            var cusName = t.FindElementWithoutException(By.XPath(".//span[@class = 'customer']/a"))?.Text.Trim() ?? "";
            var cusInn = t.FindElementWithoutException(By.XPath(".//span[@class = 'customer']/following-sibling::span"))
                ?.Text.Replace("ИНН", "").Trim() ?? "";
            var nmck = t.FindElementWithoutException(By.XPath(".//td/span[contains(@class, 'nmck')]"))?.Text
                           .DelAllWhitespace().Trim() ??
                       "";
            var tt = new TypeMzVoron
            {
                PurName = purName,
                PurNum = purNum,
                CusInn = cusInn,
                CusName = cusName,
                DatePub = datePub,
                DateEnd = dateEnd,
                Nmck = nmck,
                Status = status,
                Href = href
            };
            _tendersList.Add(tt);
        }
    }
}