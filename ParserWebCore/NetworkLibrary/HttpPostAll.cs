using System.Net.Http;

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostAll
    {
        public static HttpPostAll CreateInstance()
        {
            return new HttpPostAll();
        }

        public string DownloadString(string url)
        {
            using (var client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
            }))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0");
                var response = client.GetAsync(url);
                var res = response.Result;
                return res.Content.ReadAsStringAsync().Result;
            }
        }
    }
}