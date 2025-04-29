#region

using System;
using System.Net;
using System.Net.Http;

#endregion

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostCookies
    {
        public static HttpPostCookies CreateInstance()
        {
            return new HttpPostCookies();
        }

        public string DownloadString(string url, string baseUrl, Cookie cookie,
            FormUrlEncodedContent postContent = null)
        {
            var cookieContainer = new CookieContainer();
            //cookieContainer.SetCookies(new Uri(baseUrl), "PHPSESSID");
            cookieContainer.Add(new Uri(baseUrl), cookie);
            using (var client = new HttpClient(new HttpClientHandler
                   {
                       AllowAutoRedirect = true,
                       CookieContainer = cookieContainer,
                       UseCookies = true
                   }))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.41 Safari/537.36");
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