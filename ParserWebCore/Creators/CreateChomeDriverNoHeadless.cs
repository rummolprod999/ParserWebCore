using System;
using OpenQA.Selenium.Chrome;
using ParserWebCore.Logger;

namespace ParserWebCore.Creators
{
    public class CreateChomeDriverNoHeadless
    {
        private static ChromeDriver _driver;

        static CreateChomeDriverNoHeadless()
        {
            try
            {
                var options = new ChromeOptions();
                //options.AddArguments("headless");
                options.AddArguments("disable-gpu");
                options.AddArguments("no-sandbox");
                options.AddArguments(
                    "user-agent=Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
                //options.AddArguments("remote-debugging-port=9222");
                _driver = new ChromeDriver("/usr/local/bin", options);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                //Driver.Manage().Window.Maximize();
                _driver.Manage().Cookies.DeleteAllCookies();
            }
            catch (Exception e)
            {
                Log.Logger(e);
                throw;
            }
        }

        public static ref ChromeDriver GetChromeDriver()
        {
            return ref _driver;
        }
    }
}