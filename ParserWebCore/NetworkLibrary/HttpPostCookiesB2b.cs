using System;
using System.Net;
using System.Net.Http;

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostCookiesB2b
    {
        public static HttpPostCookiesB2b CreateInstance()
        {
            return new HttpPostCookiesB2b();
        }

        public string DownloadString(string url, CookieCollection cookie,
            FormUrlEncodedContent postContent = null)
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://www.b2b-center.ru/"), cookie);
            using (var client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = cookieContainer,
                UseCookies = true
            }))
            {
                //client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent",
                    RandomUa.RandomUserAgent);
                var response = client.GetAsync(url);
                var res = response.Result;
                if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine(res.StatusCode);
                }

                return res.Content.ReadAsStringAsync().Result;
            }
        }
    }
}