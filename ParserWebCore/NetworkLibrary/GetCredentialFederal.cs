using System;
using System.Net;
using System.Net.Http;

namespace ParserWebCore.NetworkLibrary
{
    public class GetCookieServiceFederal : CookieService
    {
        private static GetCookieServiceFederal service = new GetCookieServiceFederal();
        private readonly string BaseUrl = "https://t2.federal1.ru";

        private GetCookieServiceFederal()
        {
        }

        public Cookie Credential()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler {CookieContainer = cookieContainer};
            var client = new HttpClient(handler);
            var response = client.GetAsync(BaseUrl);
            var res = response.Result;
            _ = res.Content.ReadAsStringAsync().Result;
            var cookies = handler.CookieContainer.GetCookies(new Uri(BaseUrl));
            var cookie = cookies["PHPSESSID"];
            Console.WriteLine(cookie.Value);
            return null;
        }

        public static GetCookieServiceFederal CreateInstance()
        {
            return service;
        }
    }
}