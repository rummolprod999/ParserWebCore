using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ParserWebCore.BuilderApp;

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostSpg
    {
        public static HttpPostSpg CreateInstance()
        {
            return new HttpPostSpg();
        }

        public string DownloadString(string url)
        {
            var cookieContainer = new CookieContainer();
            //cookieContainer.SetCookies(new Uri(baseUrl), "PHPSESSID");
            using (var client = new HttpClient(new HttpClientHandler
                   {
                       AllowAutoRedirect = true,
                       CookieContainer = cookieContainer,
                       UseCookies = true,
                       ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                   }))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.41 Safari/537.36");
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("AUTH_FORM", "Y"),
                    new KeyValuePair<string, string>("TYPE", "AUTH"),
                    new KeyValuePair<string, string>("USER_REMEMBER", "Y"),
                    new KeyValuePair<string, string>("USER_LOGIN", AppBuilder.SpgrUser),
                    new KeyValuePair<string, string>("USER_PASSWORD", AppBuilder.SpgrPass),
                    new KeyValuePair<string, string>("Login", "%D0%92%D0%BE%D0%B9%D1%82%D0%B8")
                });
                var response = client.PostAsync(url, content);
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