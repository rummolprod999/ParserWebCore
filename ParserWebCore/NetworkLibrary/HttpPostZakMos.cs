#region

using System.Net.Http;
using System.Text;

#endregion

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostZakMos
    {
        public string DownloadString(string url, string data)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                var response = client.PostAsync(
                    url,
                    new StringContent(data, Encoding.UTF8, "application/json"));
                var res = response.Result;
                return res.Content.ReadAsStringAsync().Result;
            }
        }
    }
}