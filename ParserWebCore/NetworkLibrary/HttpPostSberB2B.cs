using System.Net.Http;
using System.Text;

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostSberB2B
    {
        private readonly int _page;

        public HttpPostSberB2B(int page)
        {
            _page = page;
        }

        public string DownloadString(string url)
        {
            var myJson =
                "{\"orderBy\":{\"r_published_at\":\"desc\"},\"selectBy\":{\"like_concat\":{\"r_name\":[\"\",[{\"name\":\"r.numericHash\",\"is_varchar\":false},{\"name\":\"r.name\",\"is_varchar\":true},{\"name\":\"customer_company.shortName\",\"is_varchar\":true}]]}},\"pagination\":{\"page\":" +
                _page + ",\"size\":20},\"extra\":{}}";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                var response = client.PostAsync(
                    url,
                    new StringContent(myJson, Encoding.UTF8, "application/json"));
                var res = response.Result;
                return res.Content.ReadAsStringAsync().Result;
            }
        }
    }
}