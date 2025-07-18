﻿#region

using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ParserWebCore.Logger;

#endregion

namespace ParserWebCore.Extensions
{
    public static class ChromeDriverExtensions
    {
        public static void Clicker(this ChromeDriver driver, string findPath)
        {
            //var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            var breakIt = true;
            var count = 0;
            while (breakIt)
            {
                try
                {
                    /*wait.Until(dr =>
                        dr.FindElement(By.XPath(findPath)).Displayed);*/
                    driver.FindElement(By.XPath(findPath)).Click();
                    driver.SwitchTo().DefaultContent();
                    breakIt = false;
                }
                catch (Exception e)
                {
                    count++;
                    Log.Logger(e);
                    if (count > 10)
                    {
                        return;
                    }
                }
            }
        }

        public static void ExecutorJs(this ChromeDriver driver, string findPath)
        {
            var breakIt = true;
            var count = 0;
            while (breakIt)
            {
                try
                {
                    var js = driver as IJavaScriptExecutor;
                    js.ExecuteScript(findPath);
                    driver.SwitchTo().DefaultContent();
                    breakIt = false;
                }
                catch (Exception e)
                {
                    count++;
                    Log.Logger(e);
                    if (count > 10)
                    {
                        return;
                    }
                }
            }
        }

        public static IWebElement FindElementWithoutException<T>(this T driver, By by) where T : ISearchContext
        {
            try
            {
                return driver.FindElement(by);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}