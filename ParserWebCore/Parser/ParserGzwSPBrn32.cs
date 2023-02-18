using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;

namespace ParserWebCore.Parser
{
    public class ParserGzwSPBrn32 : ParserGzwSp
    {
        public ParserGzwSPBrn32(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg,
            int count) :
            base(url, baseurl, etpName, etpUrl, typeFz, arg, count)
        {
        }

        public override void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            driver.Navigate()
                .GoToUrl(
                    "http://tender32.ru/smallpurchases/Login/Form?err=badlogged&ret=%2fsmallpurchases%2fGzwSP%2fBidCreate%3fnoticeLink%3d1047872");
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//input[@name = 'login']")));
            Thread.Sleep(1000);
            driver.SwitchTo().DefaultContent();
            driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.Brn32User);
            driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(AppBuilder.Brn32Pass);
            driver.FindElement(By.XPath("//input[@value = 'Вход']")).Click();
            Thread.Sleep(5000);
            AuthCookieValue = driver.Manage().Cookies.GetCookieNamed("ebudget").Value;
        }
    }
}