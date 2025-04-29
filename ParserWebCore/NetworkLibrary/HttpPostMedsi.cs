#region

using System.Net.Http;
using System.Text;

#endregion

namespace ParserWebCore.NetworkLibrary
{
    public class HttpPostMedsi
    {
        private readonly int _page;

        public HttpPostMedsi(int page)
        {
            _page = page;
        }

        public string DownloadString(string url)
        {
            var myJson =
                $"------WebKitFormBoundaryfEszbThD8WY28JUd\\r\\nContent-Disposition: form-data; name=\"page\"\\r\\n\\r\\n{_page}\\r\\n------WebKitFormBoundaryfEszbThD8WY28JUd\\r\\nContent-Disposition: form-data; name=\"per_page\"\\r\\n\\r\\n12\\r\\n------WebKitFormBoundaryfEszbThD8WY28JUd\\r\\nContent-Disposition: form-data; name=\"id_zp\"\\r\\n\\r\\n\\r\\n------WebKitFormBoundaryfEszbThD8WY28JUd\\r\\nContent-Disposition: form-data; name=\"type_of_service\"\\r\\n\\r\\n\\r\\n------WebKitFormBoundaryfEszbThD8WY28JUd\\r\\nContent-Disposition: form-data; name=\"date_active_from_1\"\\r\\n\\r\\n\\r\\n------WebKitFormBoundaryfEszbThD8WY28JUd\\r\\nContent-Disposition: form-data; name=\"date_active_from_2\"\\r\\n\\r\\n\\r\\n------WebKitFormBoundaryfEszbThD8WY28JUd\\r\\nContent-Disposition: form-data; name=\"dependent_group\"\\r\\n\\r\\n\\r\\n------WebKitFormBoundaryfEszbThD8WY28JUd--\\r\\n";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                var response = client.PostAsync(
                    url,
                    new StringContent(myJson, Encoding.UTF8));
                var res = response.Result;
                return res.Content.ReadAsStringAsync().Result;
            }
        }
    }
}