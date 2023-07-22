using System;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.Creators;
using ParserWebCore.Extensions;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;
using ParserWebCore.Tender;
using ParserWebCore.TenderType;

namespace ParserWebCore.Parser
{
    public class ParserStPo : ParserAbstract, IParser
    {
        public static string cookie = "";
        public static string ua = "";
        private readonly ChromeDriver _driver = CreateChomeDriverNoHeadless.GetChromeDriver();
        private TimeSpan _timeoutB = TimeSpan.FromSeconds(60);

        public void Parsing()
        {
            Parse(ParsingStpo);
        }

        private void ParsingStpo()
        {
            try
            {
                var wait = new WebDriverWait(_driver, _timeoutB);
                _driver.Navigate().GoToUrl("https://www.stroyportal.ru/tender/?region_id=all&type=");
                Thread.Sleep(5000);
                var cookies = _driver.Manage().Cookies.AllCookies;
                var str = new StringBuilder();
                foreach (var cookie in cookies)
                {
                    str.Append($"{cookie.Name}={cookie.Value};");
                }

                cookie = str.ToString();
                Console.WriteLine(cookie);
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

            ParsingPage($"https://www.stroyportal.ru/tender/?region_id=all&type=");
            for (int i = 45; i <= 45 * 5; i = i + 45)
            {
                try
                {
                    ParsingPage($"https://www.stroyportal.ru/tender/st{i}/?type=&region_id=all");
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParsingPage(string url)
        {
            var arg =
                $"\"{url}\" -H \"cookie: {cookie}\" -H \"user-agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36\"";
            Console.WriteLine(arg);
            var s = CurlDownloadSportMaster.DownL(arg);
            if (string.IsNullOrEmpty(s))
            {
                Log.Logger("Empty string in ParserPage()", url);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);
            var tens =
                htmlDoc.DocumentNode.SelectNodes("//a[contains(@class, 'tender_item')]") ??
                new HtmlNodeCollection(null);
            foreach (var a in tens)
            {
                try
                {
                    ParserTender(a);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void ParserTender(HtmlNode n)
        {
            var purName = (n.SelectSingleNode(".//span[contains(@class, 'tender_title')]")
                ?.InnerText ?? "").Trim().DelDoubleWhitespace();
            if (string.IsNullOrEmpty(purName))
            {
                Log.Logger("Empty purName");
                return;
            }

            var purNum = (n.SelectSingleNode(".//span[contains(@class, 'font-10 d-block font-lightgray top-4')]")
                ?.InnerText ?? "").Trim().DelDoubleWhitespace();
            if (string.IsNullOrEmpty(purNum))
            {
                Log.Logger("Empty purNum");
                return;
            }

            var region = (n.SelectSingleNode("./span/span[4]")
                ?.InnerText ?? "").Trim().DelDoubleWhitespace();
            var href =
                n?.Attributes["href"]?.Value ??
                throw new Exception(
                    $"Cannot find href in {purNum}");
            var dateEndT =
                (n.SelectSingleNode(".//span[contains(@class, 'd-flex top-8 font-12 font-lightgray')]/span[2]")
                    ?.InnerText ?? "").Trim().Replace(", актуально до", "").DelDoubleWhitespace().Trim();
            dateEndT = dateEndT.GetDateWithMonthFull() + " " + DateTime.Today.Year;
            var dateEnd = dateEndT.ParseDateUn("d MM yyyy");
            var tn = new TenderStPo("Стройпортал.ру", "https://www.stroyportal.ru/", 404,
                new TypeStPo
                {
                    DateEnd = dateEnd,
                    DatePub = DateTime.Now,
                    Href = href,
                    PurNum = purNum,
                    PurName = purName,
                    Region = region
                });
            ParserTender(tn);
        }
    }
}