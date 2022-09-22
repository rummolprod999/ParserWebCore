using System;
using System.Net;
using System.Net.Http;
using ParserWebCore.BuilderApp;

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
            var handler = new HttpClientHandler { CookieContainer = cookieContainer, AllowAutoRedirect = true };
            if (AppBuilder.UserProxy)
            {
                var prixyEntity = ProxyLoader.getRandomProxy();
                var proxy = new WebProxy
                {
                    Address = new Uri($"http://{prixyEntity.Ip}:{prixyEntity.Port}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        userName: prixyEntity.User,
                        password: prixyEntity.Pass)
                };
                handler.Proxy = proxy;
            }

            var client = new HttpClient(handler);
            HttpPostCookiesB2b.FillUserAgent(client, null);
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