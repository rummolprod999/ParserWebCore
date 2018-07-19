using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.Creators;
using ParserWebCore.Logger;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserAgroTomsk: ParserAbstract, IParser
    {
        private const int Count = 2;
        private TimeSpan timeoutB = TimeSpan.FromSeconds(120);
        private const string Url = "http://agro.zakupki.tomsk.ru/Competition/Competition_Request_Cost.aspx?Sale=0";
        private List<TypeAgroTomsk> _listTenders = new List<TypeAgroTomsk>();
        private readonly ChromeDriver _driver = CreatorChromeDriver.GetChromeDriver();
        
        public void Parsing()
        {
            Parse(ParsingAgroTomsk);
        }

        private void ParsingAgroTomsk()
        {
            try
            {
                ParserSelenium();
            }
            catch (Exception e)
            {
                Log.Logger(e);
            }
            finally
            {
                _driver.Manage().Cookies.DeleteAllCookies();
                _driver.Quit();
            }
        }

        private void ParserSelenium()
        {
            var wait = new WebDriverWait(_driver, timeoutB);
            _driver.Navigate().GoToUrl(Url);
            Thread.Sleep(5000);
            wait.Until(dr => dr.FindElement(By.XPath("//table[@id = 'MainContent_dgProducts']//tr[@class = 'ltint'][20]")));
            ParserFirstPage();
        }

        private void ParserFirstPage()
        {
            var tenders =
                _driver.FindElements(By.XPath("//table[@id = 'MainContent_dgProducts']//tr[@class = 'ltint']"));
            foreach (var t in tenders)
            {
                try
                {
                    ParsingPage(t);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(IWebElement t)
        {
            var purNum = t.FindElement(By.XPath("./td[2]"))?.Text.Trim() ?? "";
            Console.WriteLine(purNum);
        }
    }
}