#region

using System;
using System.Net;
using System.Net.Http;

#endregion

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostCookiesAll
    {
        public static HttpPostCookiesAll CreateInstance()
        {
            return new HttpPostCookiesAll();
        }

        public string DownloadString(string url, string baseUrl, CookieCollection cookie,
            FormUrlEncodedContent postContent = null, bool useProxy = false)
        {
            var cookieContainer = new CookieContainer();
            //cookieContainer.SetCookies(new Uri(baseUrl), "PHPSESSID");
            cookieContainer.Add(new Uri(baseUrl), cookie);
            var httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = cookieContainer,
                UseCookies = true,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
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