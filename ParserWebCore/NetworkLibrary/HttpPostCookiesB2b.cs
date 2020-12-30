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
                FillUserAgent(client);
                var response = client.GetAsync(url);
                var res = response.Result;
                if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine(res.StatusCode);
                }

                return res.Content.ReadAsStringAsync().Result;
            }
        }

        protected internal static void FillUserAgent(HttpClient client)
        {
            try
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    RandomUa.RandomUserAgent);
            }
            catch (Exception)
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.50 Safari/537.36");
            }
        }
    }
}