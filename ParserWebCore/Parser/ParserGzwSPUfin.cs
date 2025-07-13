#region

using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using ParserWebCore.Extensions;
using ParserWebCore.Tender;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserGzwSPUfin : ParserGzwSp
    {
        public ParserGzwSPUfin(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg,
            int count) :
            base(url, baseurl, etpName, etpUrl, typeFz, arg, count)
        {
        }

        public override void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            driver.Navigate()
                .GoToUrl(
                    "https://goszakaz.ufin48.ru/smallpurchases/Login/Form?err=badlogged&ret=%2fsmallpurchases%2fProfile%2fGotoHomePage");
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//input[@name = 'login']")));
            Thread.Sleep(3000);
            driver.SwitchTo().DefaultContent();
            try
            {
                var alert = driver.SwitchTo().Alert();
                alert.Dismiss();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Thread.Sleep(3000);
            driver.SwitchTo().DefaultContent();
            try
            {
                wait.Until(dr =>
                    dr.FindElement(By.XPath(
                        "//button[. = 'Закрыть']")));
                driver.FindElement(By.XPath("//button[. = 'Закрыть']")).Click();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                driver.ExecutorJs(
                    "var elem = document.querySelectorAll('button.ui-button.ui-corner-all.ui-widget'); elem[elem.length-1].click()");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            /*driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.UfinUser);
            driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(AppBuilder.UfinPass);
            driver.FindElement(By.XPath("//input[@value = 'Вход']")).Click();*/
            try
            {
                driver.SwitchTo().DefaultContent();
                driver.ExecutorJs(
                    $"var elem = document.querySelectorAll('input[name=\\'login\\']'); elem[elem.length-1].value = \"{AppBuilder.UfinUser}\"");
                driver.ExecutorJs(
                    $"var elem = document.querySelectorAll('input[name=\\'pass\\']'); elem[elem.length-1].value = \"{AppBuilder.UfinPass}\"");
                driver.ExecutorJs(
                    "var elem = document.querySelectorAll('input[value=\\'Вход\\']'); elem[elem.length-2].click()");
                driver.SwitchTo().DefaultContent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Thread.Sleep(15000);
            AuthCookieValue = driver.Manage().Cookies.GetCookieNamed("ebudget").Value;
        }

        protected override void ParserTender(ITender t)
        {
            base.ParserTender(t);
            Thread.Sleep(10000);
        }
    }
}