#region

using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using Cookie = System.Net.Cookie;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserGzwSPDvina : ParserGzwSp
    {
        public ParserGzwSPDvina(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg,
            int count) :
            base(url, baseurl, etpName, etpUrl, typeFz, arg, count)
        {
        }

        public override void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            driver.Navigate()
                .GoToUrl(
                    "https://zakupki.dvinaland.ru/smallpurchases/Login/Form?err=badlogged&ret=%2fsmallpurchases%2fProfile%2fGotoHomePage");
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//input[@name = 'login']")));
            Thread.Sleep(1000);
            driver.SwitchTo().DefaultContent();
            driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.DvinaUser);
            driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(AppBuilder.DvinaPass);
            driver.FindElement(By.XPath("//input[@value = 'Вход']")).Click();
            Thread.Sleep(5000);
            foreach (var cookiesAllCookie in driver.Manage().Cookies.AllCookies)
            {
                col.Add(new Cookie(cookiesAllCookie.Name, cookiesAllCookie.Value));
            }
        }
    }
}