using System.Net.Http;
using System.Net.Http.Headers;
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
                $"{{\"orderBy\":{{\"r_published_at\":\"desc\"}},\"selectBy\":{{\"like_concat\":{{\"r_name\":[\"\",[{{\"name\":\"r.number\",\"is_varchar\":false}},{{\"name\":\"r.name\",\"is_varchar\":true}},{{\"name\":\"customer_company.shortName\",\"is_varchar\":true}},{{\"name\":\"customer_company.inn\",\"is_varchar\":true}}]]}}}},\"pagination\":{{\"page\":{_page},\"size\":10}},\"extra\":{{\"subjectDomain\":\"\"}}}}";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.PostAsync(
                    url,
                    new StringContent(myJson, Encoding.UTF8, "application/json"));
                var res = response.Result;
                return res.Content.ReadAsStringAsync().Result;
            }
        }
    }
}