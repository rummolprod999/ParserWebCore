using System;
using System.Net;
using ParserWebCore.Logger;
using ParserWebCore.NetworkLibrary;

namespace ParserWebCore.Parser
{
    public class ParserFederal : ParserAbstract, IParser
    {
        private static Cookie cookie;
        public static readonly string HttpsT2Federal1Ru = "https://t2.federal1.ru/";
        private readonly CookieService _cookieService = GetCookieServiceFederal.CreateInstance();

        public void Parsing()
        {
            Parse(ParsingFederal);
        }

        private void ParsingFederal()
        {
            try
            {
                GetPage();
            }
            catch (Exception e)
            {
                Log.Logger($"Error in {GetType().Name}.{System.Reflection.MethodBase.GetCurrentMethod().Name}", e);
            }
        }

        private void GetPage()
        {
            cookie = _cookieService.CookieValue();
            for (var i = 1; i < 6; i++)
            {
                GetPage($"https://t2.federal1.ru/registry/list/?page={i}");
            }
        }

        private void GetPage(string url)
        {
            var s = DownloadString.DownLHttpPostWithCookies(url, HttpsT2Federal1Ru, cookie);
            Console.WriteLine(s);
        }
    }
}