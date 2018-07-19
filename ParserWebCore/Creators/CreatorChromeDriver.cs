using System;
using OpenQA.Selenium.Chrome;

namespace ParserWebCore.Creators
{
    public static class CreatorChromeDriver
    {
        private static readonly ChromeDriver Driver;
        static CreatorChromeDriver()
        {
            var options = new ChromeOptions();
            //options.AddArguments("headless");
            options.AddArguments("disable-gpu");
            options.AddArguments("no-sandbox");
            Driver = new ChromeDriver("/usr/local/bin", options);
            Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
            Driver.Manage().Window.Maximize();
            Driver.Manage().Cookies.DeleteAllCookies();
        }

        public static ChromeDriver GetChromeDriver()
        {
            return Driver;
        }
    }
}