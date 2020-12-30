using System;
using System.Net;
using System.Net.Http;

namespace ParserWebCore.NetworkLibrary
{
    public class CookiesB2B
    {
        private static CookiesB2B service = new CookiesB2B();
        private readonly string BaseUrl = "https://www.b2b-center.ru/market/";

        private CookiesB2B()
        {
        }

        public CookieCollection CookieValue()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler {CookieContainer = cookieContainer};
            var client = new HttpClient(handler);
            HttpPostCookiesB2b.FillUserAgent(client);
            var response = client.GetAsync(BaseUrl);
            var res = response.Result;
            _ = res.Content.ReadAsStringAsync().Result;
            var cookies = handler.CookieContainer.GetCookies(new Uri(BaseUrl));
            return cookies;
        }

        public static CookiesB2B CreateInstance()
        {
            return service;
        }
    }
}