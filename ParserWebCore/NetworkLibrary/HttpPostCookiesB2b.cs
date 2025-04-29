#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

#endregion

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostCookiesB2b
    {
        public static HttpPostCookiesB2b CreateInstance()
        {
            return new HttpPostCookiesB2b();
        }

        public string DownloadString(string url, CookieCollection cookie = null,
            FormUrlEncodedContent postContent = null, bool useProxy = false, Dictionary<string, string> headers = null)
        {
            var cookieContainer = new CookieContainer();
            if (cookie != null)
            {
                cookieContainer.Add(new Uri("https://www.b2b-center.ru/"), cookie);
            }

            var httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = cookieContainer,
                UseCookies = true
            };
            if (useProxy)
            {
                var prixyEntity = ProxyLoader.getRandomProxy();
                var proxy = new WebProxy
                {
                    Address = new Uri($"http://{prixyEntity.Ip}:{prixyEntity.Port}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        prixyEntity.User,
                        prixyEntity.Pass)
                };
                httpClientHandler.Proxy = proxy;
            }

            using (var client = new HttpClient(httpClientHandler))
            {
                //client.DefaultRequestHeaders.Clear();
                FillUserAgent(client, headers);
                var response = client.GetAsync(url);
                var res = response.Result;
                if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine(res.StatusCode);
                }

                return res.Content.ReadAsStringAsync().Result;
            }
        }

        protected internal static void FillUserAgent(HttpClient client, Dictionary<string, string> dictionary)
        {
            try
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    RandomUa.RandomUserAgent);
                if (dictionary != null)
                {
                    foreach (var keyValuePair in dictionary)
                    {
                        client.DefaultRequestHeaders.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }
            }
            catch (Exception)
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.50 Safari/537.36");
            }
        }
    }
}