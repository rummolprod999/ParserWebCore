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
    public class ParserGzwSPTver : ParserGzwSp
    {
        public ParserGzwSPTver(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg,
            int count) :
            base(url, baseurl, etpName, etpUrl, typeFz, arg, count)
        {
        }

        public override void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            driver.Navigate()
                .GoToUrl(
                    "https://www.tver.ru/zakaz/GzwSP/NoticesGrid?ItemId=342&show_title=on");
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//input[@name = 'login']")));
            Thread.Sleep(1000);
            driver.SwitchTo().DefaultContent();
            driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.UdmUser);
            driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(AppBuilder.UdmPass);
            driver.FindElement(By.XPath("//button[. = 'Вход']")).Click();
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