using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;

namespace ParserWebCore.Parser
{
    public class ParserGzwSPUfin : ParserGzwSp
    {
        public ParserGzwSPUfin(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg) :
            base(url, baseurl, etpName, etpUrl, typeFz, arg)
        {
        }

        public override void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            driver.Navigate().GoToUrl(_url);
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//input[@name = 'login']")));
            Thread.Sleep(1000);
            driver.SwitchTo().DefaultContent();
            driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(Builder.UfinUser);
            driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(Builder.UfinPass);
            driver.FindElement(By.XPath("//input[@value = 'Вход']")).Click();
            Thread.Sleep(5000);
        }
    }
}