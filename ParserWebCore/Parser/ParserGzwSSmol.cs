using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParserWebCore.BuilderApp;
using TwoCaptcha.Captcha;

namespace ParserWebCore.Parser
{
    public class ParserGzwSSmol : ParserGzwSp
    {
        public ParserGzwSSmol(string url, string baseurl, string etpName, string etpUrl, int typeFz, Arguments arg,
            int count = 10) :
            base(url, baseurl, etpName, etpUrl, typeFz, arg, count)
        {
        }

        public override void Auth(ChromeDriver driver, WebDriverWait wait)
        {
            driver.Navigate().GoToUrl("https://goszakupki.admin-smolensk.ru/smallpurchases/Login/Form");
            wait.Until(dr =>
                dr.FindElement(By.XPath(
                    "//input[@name = 'login']")));
            //Thread.Sleep(1000);
            var solver = new TwoCaptcha.TwoCaptcha(AppBuilder.Api);
            solver.DefaultTimeout = 120;
            solver.RecaptchaTimeout = 600;
            solver.PollingInterval = 10;
            var base64string = driver.ExecuteScript(@"
    var c = document.createElement('canvas');
    var ctx = c.getContext('2d');
    var img = document.getElementById('captcha');
    c.height=img.naturalHeight;
    c.width=img.naturalWidth;
    ctx.drawImage(img, 0, 0,img.naturalWidth, img.naturalHeight);
    var base64String = c.toDataURL();
    return base64String;
    ") as string;

            var base64 = base64string.Split(',').Last();
            using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
            {
                using (var bitmap = new Bitmap(stream))
                {
                    var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Captcha.jpeg");
                    bitmap.Save(filepath, ImageFormat.Jpeg);
                }
            }

            var captcha = new Normal();
            captcha.SetFile("Captcha.jpeg");
            captcha.SetMinLen(3);
            captcha.SetMaxLen(20);
            captcha.SetCaseSensitive(true);
            solver.Solve(captcha).GetAwaiter().GetResult();
            Console.WriteLine(captcha.Code);
            driver.SwitchTo().DefaultContent();
            driver.FindElement(By.XPath("//input[@name = 'login']")).SendKeys(AppBuilder.SmolUser);
            driver.FindElement(By.XPath("//input[@name = 'pass']")).SendKeys(AppBuilder.SmolPass);
            driver.FindElement(By.XPath("//input[@name = 'captcha']")).SendKeys(captcha.Code);
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