using System.Net.Http;
using System.Text;

namespace ParserWebCore.NetworkLibrary
{
    public class HttpZmoRts
    {
        public HttpZmoRts()
        {
        }

        public string DownloadString(string url, string data, int section)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("xxx-tenantid-header", section.ToString());
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.56 Safari/537.36");
                var response = client.PostAsync(
                    url,
                    new StringContent(data, Encoding.UTF8, "application/json"));
                var res = response.Result;
                return res.Content.ReadAsStringAsync().Result;
            }
        }
    }
}