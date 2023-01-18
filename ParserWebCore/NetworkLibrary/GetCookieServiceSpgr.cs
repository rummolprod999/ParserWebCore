using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ParserWebCore.BuilderApp;

namespace ParserWebCore.NetworkLibrary
{
    public class GetCookieServiceSpgr
    {
        private static GetCookieServiceSpgr service = new GetCookieServiceSpgr();
        private readonly string BaseUrl = "https://procurement.spgr.ru/tender/?login=yes";

        private GetCookieServiceSpgr()
        {
        }

        public CookieCollection CookieValue()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookieContainer };
            var client = new HttpClient(handler);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("AUTH_FORM", "Y"),
                new KeyValuePair<string, string>("TYPE", "AUTH"),
                new KeyValuePair<string, string>("USER_REMEMBER", "Y"),
                new KeyValuePair<string, string>("USER_LOGIN", AppBuilder.SpgrUser),
                new KeyValuePair<string, string>("USER_PASSWORD", AppBuilder.SpgrPass),
                new KeyValuePair<string, string>("Login", "%D0%92%D0%BE%D0%B9%D1%82%D0%B8")
            });
            var response = client.PostAsync(BaseUrl, content);
            var res = response.Result;
            _ = res.Content.ReadAsStringAsync().Result;
            var cookies = handler.CookieContainer.GetCookies(new Uri("https://procurement.spgr.ru/"));
            return cookies;
        }

        public static GetCookieServiceSpgr CreateInstance()
        {
            return service;
        }
    }
}