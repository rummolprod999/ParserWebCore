#region

using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using ParserWebCore.Tender;

#endregion

namespace ParserWebCore.Parser
{
    public class ParserGzwKurgan : ParserGzwSp
    {
        public ParserGzwKurgan(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg,
            int count) :
            base(url, baseurl, etpName, etpUrl, typeFz, arg, count)
        {
        }

        public override void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            driver.Navigate()
                .GoToUrl(
                    "https://zakupki.45fin.ru/smallpurchases/Login/Form?err=badlogged&ret=%2fsmallpurchases%2fGzwSP%2fBidCreate%3fnoticeLink%3dundefined");
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//input[@name = 'login']")));
            Thread.Sleep(1000);
            driver.SwitchTo().DefaultContent();
            driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.KurgUser);
            driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(AppBuilder.KurgPass);
            driver.FindElement(By.XPath("//input[@value = 'Вход']")).Click();
            Thread.Sleep(5000);
            AuthCookieValue = driver.Manage().Cookies.GetCookieNamed("ebudget").Value;
        }

        protected override void ParserTender(ITender t)
        {
            base.ParserTender(t);
            Thread.Sleep(10000);
        }
    }
}