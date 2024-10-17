using System;
using OpenQA.Selenium.Chrome;
using ParserWebCore.chrome;
using ParserWebCore.Logger;

namespace ParserWebCore.Creators
{
    public static class CreatorChromeDriverNotDetect
    {
        private static ChromeDriver _driver;
        private const string driverExecutablePath = "/usr/local/bin/patched2/chromedriver";

        static CreatorChromeDriverNotDetect()
        {
            try
            {
                var options = new ChromeOptions();
                options.AcceptInsecureCertificates = true;
                //options.AddArguments("--headless");
                options.AddArguments("--disable-gpu");
                options.AddArguments("--no-sandbox");
                options.AddArguments("--disable-dev-shm-usage");
                options.AddArguments("--ignore-certificate-errors");
                options.AcceptInsecureCertificates = true;
                _driver = UndetectedChromeDriver.Create(
                    driverExecutablePath: driverExecutablePath, options: options);
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