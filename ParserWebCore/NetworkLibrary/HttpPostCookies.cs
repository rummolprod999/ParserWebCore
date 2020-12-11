using System;
using System.Net;
using System.Net.Http;

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostCookies
    {
        public static HttpPostCookies CreateInstance()
        {
            return new HttpPostCookies();
        }

        public string DownloadString(string url, string baseUrl, Cookie cookie)
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.SetCookies(new Uri(baseUrl), "PHPSESSID");
            cookieContainer.Add(new Uri(baseUrl), cookie);
            using (var client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = cookieContainer
            }))
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0");
                var response = client.GetAsync(url);
                var res = response.Result;
                return res.Content.ReadAsStringAsync().Result;
            }
        }
    }
}