using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.Logger;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserRtsMarketNew : ParserAbstract, IParser
    {
        private const int Count = 50;
        private readonly ChromeDriver _driver;

        private readonly string[] Url =
        {
            "https://market.rts-tender.ru/search/sell?s=1&isHomeReg=false&sort=-PublicationDate&page=2",
        };

        private List<TypeSegezha> _tendersList = new List<TypeSegezha>();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(60);

        public ParserRtsMarketNew()
        {
            var options = new ChromeOptions();
            //options.AddArguments("headless");
            options.AddArguments("disable-gpu");
            options.AddArguments("no-sandbox");
            options.AddArguments("disable-infobars");
            options.AddArguments("lang=ru, ru-RU");
            options.AddArguments("window-size=1920,1080");
            options.AddArguments("disable-blink-features=AutomationControlled");
            options.AddArguments(
                "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.61 Safari/537.36");
            options.AddAdditionalCapability("useAutomationExtension", false);
            options.AddExcludedArgument("enable-automation");
            _driver = new ChromeDriver("/usr/local/bin", options);
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
            //Driver.Manage().Window.Maximize();
            _driver.Manage().Cookies.DeleteAllCookies();
        }

        public void Parsing()
        {
            Parse(ParsingRts);
        }

        private void ParsingRts()
        {
            try
            {
                foreach (var s in Url)
                {
                    ParserSelenium(s);
                }

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

        private void ParserSelenium(string Url)
        {
            var wait = new WebDriverWait(_driver, _timeoutB);
            _driver.Navigate().GoToUrl(Url);
            Thread.Sleep(5000);
        }

        private void ParserListTenders()
        {
        }
    }
}