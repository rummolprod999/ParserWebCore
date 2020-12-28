using System;
using System.IO;
using System.Net;
using System.Text;

namespace ParserWebCore.NetworkLibrary
{
    public class TimedWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = base.GetWebRequest(address);
            if (wr != null)
            {
                wr.Timeout = 20000;
                return wr;
            }

            return null;
        }
    }

    public class TimedWebClientUa : WebClient
    {
        private readonly bool _randomUa;

        public TimedWebClientUa(bool randomUa)
        {
            _randomUa = randomUa;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = (HttpWebRequest) base.GetWebRequest(address);
            if (wr != null)
            {
                wr.Timeout = 20000;
                wr.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                wr.UserAgent = _randomUa
                    ? RandomUa.RandomUserAgent
                    : "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0";
                wr.AutomaticDecompression =
                    DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None;
                return wr;
            }

            return null;
        }
    }

    public class TimedWebClientUaB2B : WebClient
    {
        private readonly bool _randomUa;

        public TimedWebClientUaB2B(bool randomUa)
        {
            _randomUa = randomUa;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = (HttpWebRequest) base.GetWebRequest(address);
            if (wr != null)
            {
                wr.Timeout = 20000;
                wr.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                wr.UserAgent = _randomUa
                    ? RandomUa.RandomUserAgent
                    : "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0";
                wr.Headers["Cookie"] =
                    "testcookie=1; lang=rus; PHPSESSID=133e1c0feb152ef7dff944773c279ab9; clrs=p9E1BDJAKxQCgijEXjMvUmNDFzyzy3Yc5JXAwWWE; ipp_uid2=v4dansSMNejTiLhW/mn20KQOizTkCYUE4PHgv1w==; ipp_uid1=1608992579134; ssrs=qJZswDpbGXI9cr1ARk7ObdAc5a8at0JhZhxX90B3; cookie_id=39dccf18; poll_participant_guest_key=5fe7474451c86811262084; tuk=db7ce121-4785-11eb-8a36-002590f3a36c; p_hint=; n_srch=0; s_srch=0; search_start_time=0; _ym_uid=1609057330747045305; _ym_d=1609057330; _fbp=fb.1.1609057333220.58399424; _gid=GA1.2.2043272305.1609167644; _ga=GA1.1.588834613.1609057330; _ym_isad=2; _ga_J6RXK7E8Q3=GS1.1.1609167643.3.0.1609167647.0; seen-cookie-message=yes; last_viewed_procedures_cookie_key_v2=2542502004%3B2535804020%3B2536227040%3B2536225040%3B50278051%3B54410053%3B54393052%3B2464189042%3B2541585038%3B2535803020%3B2536224040%3B2539948014%3B2527422004%3B2541327004%3B2541596004%3B2454329002%3B2541647026%3B2541608038%3B2541167004%3B2541142040; vc=1609171358";
                wr.AutomaticDecompression =
                    DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None;
                return wr;
            }

            return null;
        }
    }

    public class TimedWebClientTektorg : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = (HttpWebRequest) base.GetWebRequest(address);
            if (wr != null)
            {
                wr.Timeout = 20000;
                wr.UserAgent =
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.99 Safari/537.36 Vivaldi/2.9.1705.41";
                wr.Headers.Add("cookie",
                    "SL_GWPT_Show_Hide_tmp=1; SL_wptGlobTipTmp=1; Drupal.visitor.procedures_theme=blocks; _ym_uid=1572329683844748249; _ym_d=1572329683; _fbp=fb.1.1572329684066.1937010747; _ga=GA1.2.1694310890.1572329684; swp_token=1576744962:ae7ba5b4340734576bab54682f90c5be:0b5912261567c6b88f45a8c9bba719ea; _gid=GA1.2.896146885.1576743180; _ym_visorc_47749948=w; _ym_isad=2; _ym_visorc_37860345=w");
                wr.AutomaticDecompression =
                    DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None;
                return wr;
            }

            return null;
        }
    }

    public class TimedWebSber : WebClient
    {
        private readonly int _page;

        public TimedWebSber(int page)
        {
            this._page = page;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = (HttpWebRequest) base.GetWebRequest(address);
            if (wr != null)
            {
                wr.Method = "POST";
                wr.Headers.Add("x-requested-with", "XMLHttpRequest");
                wr.ContentType = "application/json";
                wr.Headers.Add("Sec-Fetch-Site", "same-origin");
                wr.Headers.Add("Sec-Fetch-Mode", "cors");
                wr.Headers.Add("Origin", "https://sberb2b.ru");
                wr.Headers.Add("Referer", "https://sberb2b.ru/request/public-requests");
                wr.Headers.Add("Cookie",
                    "SFSESSID=oaqs0kfphnf9od0casnpirvtrp; SL_GWPT_Show_Hide_tmp=1; SL_wptGlobTipTmp=1");
                wr.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                using (var streamWriter = new StreamWriter(wr.GetRequestStream()))
                {
                    var json = "{}";
                    var byteArray = Encoding.UTF8.GetBytes(json);
                    wr.ContentLength = byteArray.Length;
                    streamWriter.Write(byteArray);
                    streamWriter.Flush();
                }

                wr.Timeout = 20000;
                wr.UserAgent =
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.99 Safari/537.36 Vivaldi/2.9.1705.41";

                return wr;
            }

            return null;
        }
    }

    public class WebDownload : WebClient
    {
        public WebDownload() : this(60000)
        {
        }

        public WebDownload(int timeout)
        {
            Timeout = timeout;
        }

        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = Timeout;
            }

            return request;
        }
    }

    public class TimedWebClientFederal : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = (HttpWebRequest) base.GetWebRequest(address);
            if (wr != null)
            {
                wr.Timeout = 20000;
                wr.UserAgent =
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.99 Safari/537.36 Vivaldi/2.9.1705.41";
                wr.Headers.Add(HttpRequestHeader.Cookie,
                    "PHPSESSID=16sg2tfho0bi72hndo072ugvt3");
                wr.AutomaticDecompression =
                    DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None;
                return wr;
            }

            return null;
        }
    }
}