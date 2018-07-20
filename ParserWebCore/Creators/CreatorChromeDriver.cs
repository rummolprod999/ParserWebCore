using System;
using OpenQA.Selenium.Chrome;
using ParserWebCore.Logger;

namespace ParserWebCore.Creators
{
    public static class CreatorChromeDriver
    {
        private static ChromeDriver Driver;
        static CreatorChromeDriver()
        {
            try
            {
                var options = new ChromeOptions();
                options.AddArguments("headless");
                options.AddArguments("disable-gpu");
                options.AddArguments("no-sandbox");
                Driver = new ChromeDriver("/usr/local/bin", options);
                Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                //Driver.Manage().Window.Maximize();
                Driver.Manage().Cookies.DeleteAllCookies();
            }
            catch (Exception e)
            {
                Log.Logger(e);
                throw;
            }
        }

        public static ref ChromeDriver GetChromeDriver()
        {
            return ref Driver;
        }
    }
}