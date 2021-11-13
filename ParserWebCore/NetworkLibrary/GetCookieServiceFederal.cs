using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ParserWebCore.BuilderApp;

namespace ParserWebCore.NetworkLibrary
{
    public class GetCookieServiceFederal : CookieService
    {
        private static GetCookieServiceFederal service = new GetCookieServiceFederal();
        private readonly string BaseUrl = "https://t2.federal1.ru/login.php?externalErrMess=";

        private GetCookieServiceFederal()
        {
        }

        public Cookie CookieValue()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookieContainer };
            var client = new HttpClient(handler);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user_login", AppBuilder.FederalUser),
                new KeyValuePair<string, string>("user_pass", AppBuilder.FederalPass),
                new KeyValuePair<string, string>("submit", "%D0%92%D0%BE%D0%B9%D1%82%D0%B8")
            });
            var response = client.PostAsync(BaseUrl, content);
            var res = response.Result;
            _ = res.Content.ReadAsStringAsync().Result;
            var cookies = handler.CookieContainer.GetCookies(new Uri(BaseUrl));
            var cookie = cookies["PHPSESSID"];
            return cookie;
        }

        public static GetCookieServiceFederal CreateInstance()
        {
            return service;
        }
    }
}