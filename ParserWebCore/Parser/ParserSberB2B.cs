#region

using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Chrome;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserSberB2B : ParserAbstract, IParser
    {
        private readonly int _countPage = 20;
        private readonly string _url = "https://sberb2b.ru/request/get-public-requests?r_published_at=desc";
        private ChromeDriver _driver;
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(120);

        public void Parsing()
        {
            try
            {
                var options = new ChromeOptions();
                options.AddArguments("headless");
                options.AddArguments("disable-gpu");
                options.AddArguments("no-sandbox");
                options.AddArguments("disable-infobars");
                options.AddArguments("lang=ru, ru-RU");
                options.AddArguments("window-size=1920,1080");
                options.AddArguments("disable-blink-features=AutomationControlled");
                options.AddArguments(
                    "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.61 Safari/537.36");
                options.AddExcludedArgument("enable-automation");
                _driver = new ChromeDriver("/usr/local/bin", options);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
                //Driver.Manage().Window.Maximize();
                _driver.Manage().Cookies.DeleteAllCookies();
                Parse(ParsingSber);
            }
            finally
            {
                _driver.Manage().Cookies.DeleteAllCookies();
                _driver.Quit();
            }
        }

        private void ParsingSber()
        {
            for (var i = 1; i < _countPage; i++)
            {
                try
                {
                    GetPage(i);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}", e);
                }
            }
        }

        private void GetPage(int num)
        {
            var data =
                $"-k \"https://sberb2b.ru/request/get-public-requests?r_published_at=desc\" \\\n -H \"User-Agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36\" \\\n -H \"content-type: application/json\" \\\n  --data-raw \"{{\\\"orderBy\\\":{{\\\"r_published_at\\\":\\\"desc\\\"}},\\\"selectBy\\\":{{\\\"like_concat\\\":{{\\\"r_name\\\":[\\\"\\\",[{{\\\"name\\\":\\\"r.number\\\",\\\"is_varchar\\\":false}},{{\\\"name\\\":\\\"r.name\\\",\\\"is_varchar\\\":true}},{{\\\"name\\\":\\\"customer_company.shortName\\\",\\\"is_varchar\\\":true}},{{\\\"name\\\":\\\"customer_company.inn\\\",\\\"is_varchar\\\":true}}]]}}}},\\\"pagination\\\":{{\\\"page\\\":{num},\\\"size\\\":10}},\\\"extra\\\":{{\\\"subjectDomain\\\":\\\"\\\"}}}}\"  --compressed";
            var s = CurlDownloadSportMaster.DownL(data);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger($"Empty string in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                    _url);
                return;
            }

            var jObj = JObject.Parse(s);
            var tenders = GetElements(jObj, "data.list");
            foreach (var t in tenders)
            {
                try
                {
                    ParserTenderObj(t);
                }
                catch (Exception e)
                {
                    Log.Logger($"Error in {GetType().Name}.{MethodBase.GetCurrentMethod().Name}",
                        e);
                }
            }
        }

        private void ParserTenderObj(JToken t)
        {
            var id = ((string)t.SelectToken("id") ?? "").Trim();
            var href = $"https://sberb2b.ru/request/supplier/preview/{id}";
            var cusName = ((string)t.SelectToken("customer.short_name") ?? "").Trim();
            var purName = ((string)t.SelectToken("name") ?? "").Trim();
            var purNum = ((string)t.SelectToken("number") ?? "").Trim();
            var pubDate = (DateTime?)t.SelectToken("published_at") ?? DateTime.Now;
            var endDate = (DateTime?)t.SelectToken("send_kp_until_at") ?? DateTime.Now;
            var status = ((string)t.SelectToken("public_request_status") ?? "").Trim();
            var tn = new TenderSberB2B("SberB2B", "https://sberb2b.ru/", 220,
                new TypeSber
                {
                    Href = href,
                    Status = status,
                    PurNum = purNum,
                    DatePub = pubDate,
                    DateEnd = endDate,
                    PurName = purName,
                    CusName = cusName
                }, _driver);
            ParserTender(tn);
        }
    }
}