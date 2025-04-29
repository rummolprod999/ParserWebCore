#region

using System;
using OpenQA.Selenium.Chrome;
using ParserWebCore.Logger;

#endregion

namespace ParserWebCore.Creators
{
    public static class CreatorChromeDriverNoSsl
    {
        private static ChromeDriver _driver;

        static CreatorChromeDriverNoSsl()
        {
            try
            {
                var options = new ChromeOptions();
                options.AcceptInsecureCertificates = true;
                options.AddArguments("headless");
                options.AddArguments("disable-gpu");
                options.AddArguments("no-sandbox");
                options.AddArguments("ignore-certificate-errors");
                options.AddArguments("disable-infobars");
                options.AddArguments("lang=ru, ru-RU");
                options.AddArguments("disable-blink-features=AutomationControlled");
                options.AddArguments(
                    "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.61 Safari/537.36");
                options.AddExcludedArgument("enable-automation");
                options.AcceptInsecureCertificates = true;
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