using System;
using OpenQA.Selenium.Chrome;
using ParserWebCore.Logger;

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
                options.AddArguments("headless");
                options.AddArguments("disable-gpu");
                options.AddArguments("no-sandbox");
                options.AddArguments("ignore-certificate-errors");
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