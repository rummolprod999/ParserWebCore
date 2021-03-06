﻿using System;
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
                    "testcookie=1; lang=rus; PHPSESSID=22dc00daa0a0de3e8cd1c3ce8305dded; clrs=I9Hc6b8r7v6frmHCcnWVqPOwpAwbA4TcU8vwG0TQ; ipp_uid2=j7bhCcp9XJUCisYx/xiKBtYtBGGxJRAyVdc53lA==; ipp_uid1=1609255619477; vc=1609255619; ssrs=az9fizuv7xYcvKSh04L6INEr34gAiiYDjQxK6hPg; cookie_id=39dccf18; last_viewed_procedures_cookie_key_v2=2454329002; tuk=4b92bd94-49ea-11eb-9676-002590f39255; poll_participant_guest_key=5feb4ac65d207617071422";
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