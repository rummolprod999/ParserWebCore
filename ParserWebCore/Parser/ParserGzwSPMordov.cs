#region

using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserGzwSPMordov : ParserGzwSp
    {
        public ParserGzwSPMordov(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg,
            int count) :
            base(url, baseurl, etpName, etpUrl, typeFz, arg, count)
        {
        }

        public override void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            driver.Navigate()
                .GoToUrl(
                    "https://goszakaz44.e-mordovia.ru/smallpurchases/Login/Form?err=badlogged&ret=%2fsmallpurchases%2fProfile%2fGotoHomePage");
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//input[@name = 'login']")));
            Thread.Sleep(1000);
            driver.SwitchTo().DefaultContent();
            driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.MordovUser);
            driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(AppBuilder.MordovPass);
            driver.FindElement(By.XPath("//input[@value = 'Вход']")).Click();
            Thread.Sleep(5000);
            foreach (var cookiesAllCookie in driver.Manage().Cookies.AllCookies)
            {
                if (cookiesAllCookie.Name.Contains("ebudget"))
                {
                    AuthCookieValue = cookiesAllCookie.Value;
                    AuthCookieName = cookiesAllCookie.Name;
                }
            }
        }
    }
}