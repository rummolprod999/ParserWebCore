using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using ParserWebCore.Creators;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserZakupay : ParserAbstract, IParser
    {
        private const int Count = 10;
        private const string Url = "https://prodavay.sel-be.ru/core/supplier/registry#?category=75";
        public static CookieCollection col = new CookieCollection();
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);
        private List<TypeZakupay> tenders = new List<TypeZakupay>();


        public void Parsing()
        {
            Parse(ParsingAgro);
        }

        private void ParsingAgro()
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
            Auth();
            /*var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(Url);
            Thread.Sleep(5000);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//table[@id = 'elements-table']/tbody/tr[contains(@class, 'element-row')]")));
            for (var i = 0; i < Count; i++)
            {
                _driver.SwitchTo().DefaultContent();
                try
                {
                    _driver.ExecuteScript(
                        "window.scrollBy(0,250)", "");
                    Thread.Sleep(2000);
                    _driver.SwitchTo().DefaultContent();
                }
                catch (Exception)
                {
                    Log.Logger("This is last page, return");
                }
            }
            _driver.SwitchTo().DefaultContent();*/
            ParsingList();
            foreach (var typeZakupay in tenders)
            {
                ParserTender(new TenderZakupay("Портал Закупай", "https://prodavay.sel-be.ru/", 408,
                    typeZakupay));
            }
        }

        private void Auth()
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl("https://prodavay.sel-be.ru/core/spa2#/prodavay-desktop/login");
            Thread.Sleep(5000);
            _driver.SwitchTo().DefaultContent();
            _driver.FindElement(By.XPath("//input[@id = 'email']")).SendKeys(AppBuilder.ZakupayUser);
            _driver.FindElement(By.XPath("//input[@id = 'password']")).SendKeys(AppBuilder.ZakupayPass);
            _driver.FindElement(By.XPath("//button[@type = 'submit']")).Click();
            Thread.Sleep(5000);
            foreach (var cookiesAllCookie in _driver.Manage().Cookies.AllCookies)
            {
                col.Add(new System.Net.Cookie(cookiesAllCookie.Name, cookiesAllCookie.Value));
            }
        }

        private void ParsingList()
        {
            /*_driver.SwitchTo().DefaultContent();
            var tenders =
                _driver.FindElements(
                    By.XPath(
                        "//table[@id = 'elements-table']/tbody/tr[contains(@class, 'element-row')]"));
            foreach (var t in tenders)
            {
                try
                {
                    _driver.ExecutorJs(
                        "var elem = document.querySelectorAll('tr.element-row.collapsed.limited-element-row'); elem[0].click()");
                    //_driver.FindElement(By.XPath("(//td[@class='element-name'])[2]")).Click();
                    ParsingPage(t);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }*/
            var cookies = "";
            for (var i = 0; i < col.Count; i++)
            {
                var cc = col[i];
                cookies += cc.Name + "=" + cc.Value + "; ";
            }

            var data =
                $"\"https://prodavay.sel-be.ru/core/supplier/getorders\" \\\n -H \"content-type: application/json\" \\\n -H \"cookie: {cookies}\" \\\n --data-raw \"{{\\\"category\\\":[\\\"75\\\"],\\\"status\\\":\\\"actual\\\", \\\"size\\\":1000}}\" \\\n  --compressed";
            //Console.WriteLine(data);
            var s = CurlDownloadSportMaster.DownL(data);
            //Console.WriteLine(s);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}"
                );
                return;
            }

            var jObj = JArray.Parse(s);
            //Console.WriteLine(jObj.Count);
            var excludedId = new HashSet<string>();
            var lastid = "0";
            foreach (var t in jObj)
            {
                try
                {
                    ParserTenderObj(t, out lastid, excludedId);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                        e);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                var combined = string.Join(",", excludedId);
                var data1 =
                    $"\"https://prodavay.sel-be.ru/core/supplier/getorders\" \\\n -H \"content-type: application/json\" \\\n -H \"cookie: {cookies}\" \\\n --data-raw \"{{\\\"category\\\":[\\\"75\\\"],\\\"status\\\":\\\"actual\\\", \\\"size\\\":50,\\\"excludedIds\\\":[{combined}],\\\"lastId\\\":{lastid}}}\" \\\n  --compressed";
                //Console.WriteLine(data1);
                var s1 = CurlDownloadSportMaster.DownL(data1);
                //Console.WriteLine(s1);
                if (string.IsNullOrEmpty(s1))
                {
                    Log.Logger(
                        $"Empty string in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}"
                    );
                    return;
                }

                var jObj1 = JArray.Parse(s1);
                //Console.WriteLine(jObj1.Count);
                foreach (var t in jObj1)
                {
                    try
                    {
                        ParserTenderObj(t, out lastid, excludedId);
                    }
                    catch (Exception e)
                    {
                        Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}",
                            e);
                    }
                }
            }
        }

        private void ParserTenderObj(JToken t, out string lastid, HashSet<string> excludedId)
        {
            var id = ((string)t.SelectToken("id") ?? "").Trim();
            lastid = id;
            excludedId.Add(id);
            var purName = "Заявка " + id;
            var delivPlace = ((string)t.SelectToken("region.name") ?? "").Trim();
            var status = ((string)t.SelectToken("state") ?? "").Trim();
            var datePubT = ((string)t.SelectToken("creationDate") ?? "").Trim();
            var datePub = UnixMillisecondsToDateTime(long.Parse(datePubT));
            var dateEndT = (string)t.SelectToken("finishDate") ?? "";
            var delivTerm = "";
            var dt1 = (bool?)t.SelectToken("isDeliveryNeed") ?? false;
            var dt2 = (int?)t.SelectToken("delay") ?? 0;
            if (dt1)
            {
                delivTerm += "Доставка: Требуется,";
            }

            if (dt2 != 0)
            {
                delivTerm += " Отсрочка: " + dt2 + " дней,";
            }

            delivTerm += " Поставить к " + dateEndT;
            var ob = new List<TypeZakupay.TypeObject>();
            var cusEl = GetElements(t, "orderItems");
            cusEl.ForEach(c =>
            {
                var name = ((string)c.SelectToken("goodName") ?? "").Trim();
                var okpd = ((string)c.SelectToken("managedCategory.name") ?? "").Trim();
                var okei = ((string)c.SelectToken("unit.name") ?? "").Trim();
                var quant = ((string)c.SelectToken("count") ?? "").Trim();
                var tt = new TypeZakupay.TypeObject() { Name = name, Okei = okei, Quantity = quant, OKPD = okpd };
                ob.Add(tt);
            });
            var typeZ = new TypeZakupay()
            {
                DatePub = datePub, DateEnd = datePub.AddDays(2), PurName = purName, PurNum = id,
                Href = "https://prodavay.sel-be.ru/", Status = status, DelivPlace = delivPlace, DelivTerm = delivTerm,
                ObjectsPurchase = ob
            };
            tenders.Add(typeZ);
        }

        public DateTime UnixMillisecondsToDateTime(long timestamp, bool local = false)
        {
            var offset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
            return local ? offset.LocalDateTime : offset.UtcDateTime;
        }
    }
}